using System.Diagnostics;
using System.Net;
using System.Text;
using AutoMapper;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RazorLight;
using Yaml.Domain.K8s.Interface;
using Yaml.Infrastructure.CoustomService;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.Exception;
using Yaml.Infrastructure.YamlEum;
namespace Yaml.Domain.K8s;

public class KubeApi : IKubeApi
{
    private readonly IKuberYamlGenerator _yamlGenerator;
    private readonly IRazorLightEngine _engine;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger _logger;
    private readonly string _currentDirectory = Directory.GetCurrentDirectory();
    private readonly IConfiguration _configuration;
    private readonly Utils _utils;

    public KubeApi(
        IKuberYamlGenerator yamlGenerator, 
        IRazorLightEngine engine, 
        ILogger<KubeApi> logger,
        IMemoryCache memoryCache,
        IConfiguration config,
        Utils utils)
    {
        _yamlGenerator = yamlGenerator;
        _memoryCache = memoryCache;
        _engine = engine;
        _logger = logger;
        _configuration = config;
        _utils = utils;
    }

    private Kubernetes GetKubeClient(YamlAppInfoDto dto)
    {
        var uploadDirectory = _configuration.GetConnectionString("UploadDirectory");
        var client = _memoryCache.Get<Kubernetes>(dto.Id);
        if (client != null)
        {
            return client;
        }

        if (dto.KubeConfig == null)
        {
            throw new ServiceException("Kubernetes configuration is missing.");
        }
        try
        {
            var configFile = Path.Combine(uploadDirectory, dto.KubeConfig);
            Console.WriteLine(configFile);
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(configFile);
            client = new Kubernetes(config);
            _memoryCache.Set(dto.Id, client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Kubernetes client.");
            throw new ServiceException("Error creating Kubernetes client.", ex);
        }
        return client ?? throw new ServiceException("Client creation failed unexpectedly.");
    }
    

    public async Task<V1Namespace> CreateNamespace(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
     
            var kubeClient = GetKubeClient(dto);
            try
            {
                return await kubeClient.ReadNamespaceAsync(namespaceName, cancellationToken: cancellationToken);
            }
            catch (HttpOperationException ex) when(ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                // Namespace not found, create a new one
                _logger.LogInformation("Namespace {NamespaceName} not found. Creating new namespace", namespaceName);
                var newNamespace = new V1Namespace { Metadata = new V1ObjectMeta { Name = namespaceName } };
                var createdNamespace = await kubeClient.CreateNamespaceAsync(newNamespace, cancellationToken: cancellationToken);
                _logger.LogInformation("Namespace {NamespaceName} created successfully", namespaceName);
                return createdNamespace;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating namespace '{NamespaceName}'", namespaceName);
                throw new ServiceException($"Create namespace error {namespaceName}", e);
            }
    }

    public async Task CreateService(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var client = GetKubeClient(dto);
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;


        foreach (var cluster in dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
        {
            var serviceName = cluster.ClusterName + KubeConstants.ServiceSuffix;
            try
            {
                await client.ReadNamespacedServiceAsync(
                    serviceName,
                    namespaceName,
                    cancellationToken: cancellationToken);
                _logger.LogWarning("Service {ServiceName} already exists in namespace {NamespaceName}", serviceName, namespaceName);
            }
            catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Service {ServiceName} does not exist in namespace {NamespaceName}, creating...",
                    serviceName, namespaceName);
                var content = await _yamlGenerator.GenerateService(cluster);
                var v1Service = KubernetesYaml.Deserialize<V1Service>(content);
                await client.CreateNamespacedServiceAsync(v1Service, namespaceName,
                    cancellationToken: cancellationToken);
                _logger.LogInformation("Service {ServiceName} created successfully", serviceName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing Service {ServiceName} for app {AppName}: {Message}", serviceName, dto.AppName, e.Message);
                throw;
            }
        }
    }

    public async Task CreateDeployment(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var path = Path.Combine(_currentDirectory, KubeConstants.DeploymentTemplate);
        var client = GetKubeClient(dto);

        // create deployment for each cluster
        foreach (var cluster in dto.ClusterInfoList ??  Enumerable.Empty<YamlClusterInfoDto>())
        {
            var deploymentName = cluster.ClusterName + KubeConstants.DeploymentSuffix;
            try
            {
                // when deployment exists, update deployment
                var existingDeployment = await client.ReadNamespacedDeploymentAsync(deploymentName, namespaceName, false, cancellationToken);
                _logger.LogWarning("Deployment {DeploymentName} already exists in namespace {NamespaceName}", deploymentName, namespaceName);
                
                // Update existing deployment
                _logger.LogInformation("Updating existing deployment {DeploymentName} in namespace {NamespaceName}.", deploymentName, namespaceName);
                
                cluster.AppName = dto.AppName;
                var newContent = await _engine.CompileRenderAsync(path, cluster);
                var newDeployment = KubernetesYaml.Deserialize<V1Deployment>(newContent);

                // Update the deployment with the new spec
                existingDeployment.Spec = newDeployment.Spec;
                await client.ReplaceNamespacedDeploymentAsync(existingDeployment, deploymentName, namespaceName, cancellationToken: cancellationToken);
            }
            catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                // when deployments not exist, create
                _logger.LogInformation("Deployment {DeploymentName} does not exist in namespace {NamespaceName}, creating...", deploymentName, namespaceName);
                
                cluster.AppName = dto.AppName;
                var content = await _engine.CompileRenderAsync(path, cluster);
                var v1Deployment  = KubernetesYaml.Deserialize<V1Deployment>(content);
                await client.CreateNamespacedDeploymentAsync(v1Deployment , namespaceName, cancellationToken: cancellationToken);
               
                _logger.LogInformation("DeploymentName {DeploymentName} created successfully", deploymentName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing cluster {ClusterName} for app {AppName}: {Message}", cluster.ClusterName, dto.AppName, e.Message);
                throw;
            }
        }
    }

    public async Task CreateConfigMap(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var client = GetKubeClient(dto);
        var path = Path.Combine(_currentDirectory, KubeConstants.ConfigMapTemplate);

        // create config map 
        foreach (var cluster in dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
        {
            var configMapName = cluster.ClusterName + KubeConstants.ConfigMapSuffix;
            try
            {
                var existedConfigMap =  await client.ReadNamespacedConfigMapAsync(configMapName, namespaceName, cancellationToken: cancellationToken);
                _logger.LogWarning("ConfigMap {ConfigMap} already exists in namespace {NamespaceName}", configMapName, namespaceName);

                if (!cluster.ConfigMapFlag)
                {
                    // delete config if needed
                    break;
                }

                // Update existing configmap
                _logger.LogInformation("Updating existing configmap {configMapName} in namespace {NamespaceName}.", configMapName, namespaceName);
                cluster.AppName = dto.AppName;
                var newContent = await _engine.CompileRenderAsync(path, cluster);
                var newConfigMap = KubernetesYaml.Deserialize<V1ConfigMap>(newContent);

                // Update the configmap with the new data
                existedConfigMap.Data= newConfigMap.Data;
                await client.ReplaceNamespacedConfigMapAsync(existedConfigMap, configMapName, namespaceName, cancellationToken: cancellationToken);
                _logger.LogInformation("ConfigMap {ConfigMapName} updated successfully.", configMapName);

            }
            catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("ConfigMap {ConfigMap} does not exist in namespace {NamespaceName}, creating...", configMapName, namespaceName);

                cluster.AppName = dto.AppName;
                var content = await _engine.CompileRenderAsync(path, cluster);
                var v1ConfigMap = KubernetesYaml.Deserialize<V1ConfigMap>(content);
                await client.CreateNamespacedConfigMapAsync(v1ConfigMap, namespaceName, cancellationToken: cancellationToken);  
                
                _logger.LogInformation("ConfigMap {ConfigMap} created successfully", configMapName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateConfigMap error in application {AppName}. Error details: {ErrorMessage}", dto.AppName, ex.Message);
                throw new ServiceException($"Error in CreateConfigMap for application {dto.AppName}.", ex);
            }
        }
    }


    public async Task CreateConfigMapFile(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var client = GetKubeClient(dto);
        var path = Path.Combine(_currentDirectory, KubeConstants.ConfigFileTemplate);

        // create config map
        foreach (var cluster in dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
        {
            var configMapName = cluster.ClusterName + KubeConstants.ConfigFileSuffix;
            try
            {
                var existedConfigMap =  await client.ReadNamespacedConfigMapAsync(configMapName, namespaceName, cancellationToken: cancellationToken);
                _logger.LogWarning("ConfigMap {ConfigMap} already exists in namespace {NamespaceName}", configMapName, namespaceName);

                if (!cluster.ConfigMapFileFlag)
                {
                    // delete all config
                    await client.DeleteNamespacedConfigMapAsync(configMapName, namespaceName, body: new V1DeleteOptions { PropagationPolicy = "Foreground" });
                    return;
                }

                // Update existing configmap
                _logger.LogInformation("Updating existing configmap {configMapName} in namespace {NamespaceName}.", configMapName, namespaceName);
                cluster.AppName = dto.AppName;

                await _utils.UpdateClusterConfigFiel(cluster);
                var newContent = await _engine.CompileRenderAsync(path, cluster);
                var newConfigMap = KubernetesYaml.Deserialize<V1ConfigMap>(newContent);

                // Update the configmap with the new data
                existedConfigMap.Data= newConfigMap.Data;
                await client.ReplaceNamespacedConfigMapAsync(existedConfigMap, configMapName, namespaceName, cancellationToken: cancellationToken);
                _logger.LogInformation("ConfigMap {ConfigMapName} updated successfully.", configMapName);

            }
            catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("ConfigMap {ConfigMap} does not exist in namespace {NamespaceName}, creating...", configMapName, namespaceName);

                cluster.AppName = dto.AppName;
                await _utils.UpdateClusterConfigFiel(cluster);
                var content = await _engine.CompileRenderAsync(path, cluster);
                var v1ConfigMap = KubernetesYaml.Deserialize<V1ConfigMap>(content);
                await client.CreateNamespacedConfigMapAsync(v1ConfigMap, namespaceName, cancellationToken: cancellationToken);

                _logger.LogInformation("ConfigMap {ConfigMap} created successfully", configMapName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateConfigMap error in application {AppName}. Error details: {ErrorMessage}", dto.AppName, ex.Message);
                throw new ServiceException($"Error in CreateConfigMap for application {dto.AppName}.", ex);
            }
        }
    }

    public async Task<List<V1PersistentVolume>> CreatePersistentVolume(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        //  Create Persistent Volume
        try
        {
            // validate if the resource exists in k8s
            var client = GetKubeClient(dto);
            var pvsList = await client.ListPersistentVolumeAsync(cancellationToken: cancellationToken);
            var pvList = new List<V1PersistentVolume>();
            foreach (var cluster in dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
            {
                if (cluster.DiskInfoList?.Count > 0)
                {
                    for (var i = 0; i < cluster.DiskInfoList.Count; i++)
                    {
                        var pvName = $"{KubeConstants.PvSubPrefix}-{cluster.ClusterName?.ToLower()}-{i}";
                        var persistentVolume = pvsList.Items.SingleOrDefault(pv => pv.Metadata.Name == pvName);
                        // when pv is not found
                        if (persistentVolume == null)
                        { 
                            // if not exist, create resource
                            var diskInfo = cluster.DiskInfoList[i];
                            var storage = diskInfo.DiskSize;
                            var storageClassName = diskInfo.DiskType;
                            var subscriptionId = dto.KeyVault?.ManagedId;
                            var resourceGroup = "saas-core"; // TODO where should I get resourceGroup
                        
                            // generate pv content
                            var content = await _yamlGenerator.GeneratePersistentVolume(
                                pvName, 
                                storage, 
                                storageClassName,
                                subscriptionId, 
                                resourceGroup
                            );
                            
                            // create pv on k8s
                            var pv = KubernetesYaml.Deserialize<V1PersistentVolume>(content);
                            V1PersistentVolume persistentVolumeAsync = await client.CreatePersistentVolumeAsync(pv, cancellationToken: cancellationToken);
                            pvList.Add(persistentVolumeAsync);
                        }
                    }
                }
            }
            return pvList;
        }
        catch (Exception ex)
        {
            _logger.LogError("Create V1PersistentVolume [{}] Error", dto.AppName);
            _logger.LogError(ex, "Error details: ");
            throw new ServiceException($"Create V1PersistentVolume error {dto.AppName}", ex);
        }
    }

    public async Task CreatePersistentVolumeClaim(
        YamlAppInfoDto dto,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.PersistentVolumeClaimTemplate);
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var client = GetKubeClient(dto);
        foreach(var cluster in dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
        {
            foreach (var diskInfo in cluster.DiskInfoList)
            {
                var diskInfoDto = diskInfo;
                diskInfoDto.AppName = dto.AppName;
                var pvcName = diskInfoDto.Name + KubeConstants.PvcSubSuffix;
                try
                {
                    await client.ReadNamespacedPersistentVolumeClaimAsync(pvcName, namespaceName, cancellationToken: cancellationToken);
                    _logger.LogWarning("V1PersistentVolumeClaim {PvcName} already exists in namespace {NamespaceName}", pvcName, namespaceName);
                }
                catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("V1PersistentVolumeClaim {ConfigMap} does not exist in namespace {NamespaceName}, creating...", pvcName, namespaceName);
                
                    var content = await _engine.CompileRenderAsync(path, diskInfoDto);
                    var pvc = KubernetesYaml.Deserialize<V1PersistentVolumeClaim>(content);
                    await client.CreateNamespacedPersistentVolumeClaimAsync(pvc, namespaceName, cancellationToken: cancellationToken);
                    
                    _logger.LogInformation("V1PersistentVolumeClaim {ConfigMap} created successfully", pvcName);
                }
            }
        }
     
    }

    public async Task CreateKeyVault(YamlAppInfoDto appInfoDto, CancellationToken cancellationToken)
    {
        var namespaceName = appInfoDto.AppName + KubeConstants.NamespaceSuffix;
        var client = GetKubeClient(appInfoDto);
        
        // check if key vaults already exists
        foreach (var cluster in appInfoDto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
        {

            // generate key vault
            var keyVaultRender = GenerateKeyVaultRender(appInfoDto, cluster);

            // generate key vault parameters for spc
            var (secretObjects, objects, objectsList) = GenerateKeyVaultParams(keyVaultRender);
       
            // create Unstructured object
            var secretProviderClass = CreatedSecretProviderByCloud(appInfoDto, secretObjects, objects, objectsList, keyVaultRender);
            var kvName = $"{appInfoDto.AppName}-keyvault-spc";
            try
            {
                // create SecretProviderClass
                await client.CreateNamespacedCustomObjectAsync(
                    secretProviderClass, 
                    "secrets-store.csi.x-k8s.io",
                    "v1", 
                    secretProviderClass.Metadata?.NamespaceProperty, 
                    "secretproviderclasses",
                    cancellationToken: cancellationToken);
                _logger.LogInformation("SecretProviderClass [{}] created successfully", secretProviderClass.Metadata?.Name);
            }
            catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.Conflict)
            {
                // spc can not be replaced, so delete before creation
                _logger.LogWarning("SecretProviderClass {SecretProviderClass} already existed in namespace {NamespaceName}, deleting...", secretProviderClass.Metadata?.Name, namespaceName);
                var deleteResult = await client.DeleteNamespacedCustomObjectAsync(
                    "secrets-store.csi.x-k8s.io", 
                    "v1",
                    namespaceName,
                    "secretproviderclasses",
                    kvName,
                    cancellationToken: cancellationToken);
                _logger.LogInformation("SecretProviderClass [{}] delete successfully", kvName);
                
                // create spc
                var result = await client.CreateNamespacedCustomObjectAsync(
                    secretProviderClass, 
                    "secrets-store.csi.x-k8s.io",
                    "v1",
                    secretProviderClass.Metadata?.NamespaceProperty, 
                    "secretproviderclasses",
                     cancellationToken: cancellationToken);
                _logger.LogInformation("SecretProviderClass [{}] created successfully", kvName);
            }
        }
    }
    
    public async Task CreateIngress(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.IngressFileTemplate);
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var client = GetKubeClient(dto);
        foreach (var cluster in dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
        {
            if (!cluster.domainFlag)
            {
                continue; // 跳过当前循环迭代
            }
            var ingressName = cluster.ClusterName + KubeConstants.IngressSuffix;
            try
            {
                // when deployment exists, update deployment
                await client.ReadNamespacedIngressAsync(ingressName, namespaceName, false, cancellationToken);
                _logger.LogWarning("Ingress {IngressName} already exists in namespace {NamespaceName}",
                    ingressName, namespaceName);
            }
            catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                // when deployments not exist, create
                _logger.LogInformation(
                    "Ingress {IngressName} does not exist in namespace {NamespaceName}, creating...",
                    ingressName, namespaceName);

                cluster.AppName = dto.AppName;
                var content = await _engine.CompileRenderAsync(path, cluster);
                var ingress = KubernetesYaml.Deserialize<V1Ingress>(content);
                await client.CreateNamespacedIngressAsync(ingress, namespaceName, cancellationToken: cancellationToken);
                _logger.LogInformation("Ingress {IngressName} created successfully", ingressName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing Ingress {IngressName} for app {AppName}: {Message}",
                    cluster.ClusterName, dto.AppName, e.Message);
                throw;
            }
        }
    }

    public async Task<V1Secret[]> CreateDomainCertification(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        
        var uploadDirectory = _configuration.GetConnectionString("UploadDirectory");
        try
        {    
            var client = GetKubeClient(dto);
            var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
            var tasks = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
                .Where(cluster => cluster.domainFlag)
                .Select(async cluster => {
                    var secretName = cluster.ClusterName + "-tls-secret";
                    try
                    {
                        // Optionally skip this step if secrets are not expected to exist beforehand
                        return await client.ReadNamespacedSecretAsync(secretName, namespaceName, cancellationToken: cancellationToken);
                    }
                    catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    {
                        var certificationLocation = Path.Combine(uploadDirectory, cluster.Domain?.Certification ?? "");
                        var privateKeyLocation = Path.Combine(uploadDirectory, cluster.Domain?.PrivateKey ?? "");

                        if (!File.Exists(certificationLocation) || !File.Exists(privateKeyLocation))
                        {
                            _logger.LogError("Certificate or private key file not found for cluster {ClusterName}", cluster.ClusterName);
                            throw;
                        }
                        var certificationData =
                            await File.ReadAllTextAsync(certificationLocation, cancellationToken: cancellationToken);
                        var privateKeyData =
                            await File.ReadAllTextAsync(privateKeyLocation, cancellationToken: cancellationToken);
                        var secret = new V1Secret
                        {
                            Metadata = new V1ObjectMeta
                            {
                                Name = secretName
                            },
                            Data = new Dictionary<string, byte[]>
                            {
                                ["tls.crt"] = Encoding.UTF8.GetBytes(certificationData),
                                ["tls.key"] = Encoding.UTF8.GetBytes(privateKeyData),
                            },
                            Type = "kubernetes.io/tls"
                        };
                        return await client.CreateNamespacedSecretAsync(
                            secret,
                            namespaceName, 
                            cancellationToken: cancellationToken);
                    }
            });
            return await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating secrets for namespace: {AppName}", dto.AppName);
            throw new ServiceException("Error creating secrets", e);
        }
    }
    
        private Unstructured CreatedSecretProviderByCloud(
        YamlAppInfoDto appInfoDto,
        List<object> secretObjects,
        string objects,
        List<Dictionary<string, string>> objectsList,
        KeyVaultRender keyVaultRender)
    {
        var secretProviderClass = new Unstructured();
        if(CloudType.AWS == appInfoDto.CloudType)
        {
            secretProviderClass =
                CreateAwsServiceSecretProvider(appInfoDto, secretObjects, objectsList);
        }
        if (CloudType.Azure == appInfoDto.CloudType)
        {
            secretProviderClass = 
                CreateAzureServiceSecretProvider(appInfoDto, keyVaultRender, objects, secretObjects);
        }

        return secretProviderClass;
    }
    
    private (List<object>, string, List<Dictionary<string, string>>) GenerateKeyVaultParams(KeyVaultRender keyVaultRender)
    {
        var secretObjects = new List<object>();
        var objects = "";
        var objectsList = new List<Dictionary<string, string>>();
        foreach (var keyVault in keyVaultRender.ConfigKeyList ?? new List<string>())
        {
            secretObjects.Add(new Dictionary<string, string>
                {
                    ["key"] = keyVault,
                    ["objectName"] = keyVault
                }
            );
            objects +=
                $"      - | \n" +
                $"        objectType: secret \n" +
                $"        objectName: {keyVault} \n";
                
            var secretObject = new Dictionary<string, string>
            {
                {"objectName", keyVault},
                {"objectType", "secretsmanager"}  // 假设您要的是 secret 类型，根据实际情况调整
            };
            objectsList.Add(secretObject);
        }
        return (secretObjects, objects, objectsList);
    }

    private Unstructured CreateAzureServiceSecretProvider(
        YamlAppInfoDto appInfoDto, 
        KeyVaultRender keyVaultRender, 
        string objects, 
        List<object> secretObjects)
    {
        var secretProviderClass = new Unstructured
        {
            ApiVersion = "secrets-store.csi.x-k8s.io/v1",
            Kind = "SecretProviderClass",
            Metadata = new V1ObjectMeta
            {
                Name = $"{appInfoDto.AppName}-keyvault-spc",
                NamespaceProperty = appInfoDto.AppName  + KubeConstants.NamespaceSuffix
            },

            Spec = new Dictionary<string, object>
            {
                ["parameters"] = new Dictionary<string, object>
                {
                    ["keyvaultName"] = keyVaultRender.KeyVaultName,
                    ["objects"] = $"\n      array:\n{objects}",
                    ["tenantId"] = keyVaultRender.TenantId,
                    ["usePodIdentity"] = "false",
                    ["useVMManagedIdentity"] = "true",
                    ["userAssignedIdentityID"] = keyVaultRender.ManagedId
                },
                ["provider"] = "azure",
                ["secretObjects"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["secretName"] = $"{appInfoDto.AppName}-secret",
                        ["type"] = "Opaque",
                        ["data"] = secretObjects
                    }
                }
            }
        };
        return secretProviderClass;
    }
    
    private Unstructured CreateAwsServiceSecretProvider(
        YamlAppInfoDto appInfoDto, 
        List<object> secretObjects, 
        List<Dictionary<string, string>> objectsList)
    {
        // 创建 SecretProviderClass 的动态对象
        var secretProviderClass = new Unstructured
        {
            ApiVersion = "secrets-store.csi.x-k8s.io/v1",
            Kind = "SecretProviderClass",
            Metadata = new V1ObjectMeta
            {
                Name = $"{appInfoDto.AppName}-keyvault-spc",
                NamespaceProperty = appInfoDto.AppName  + KubeConstants.NamespaceSuffix
            },

            Spec = new Dictionary<string, object>
            {
                ["provider"] = "aws",
                ["parameters"] = new Dictionary<string, object>
                {
                    // ["region"] = "ap-northeast-1",
                    ["objects"] = JsonConvert.SerializeObject(objectsList)
                },
                ["secretObjects"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["secretName"] = $"{appInfoDto.AppName}-secret",
                        ["type"] = "Opaque",
                        ["data"] = secretObjects
                    }
                }
            }
        };
        return secretProviderClass;
    }

    private KeyVaultRender GenerateKeyVaultRender(YamlAppInfoDto appInfoDto, YamlClusterInfoDto cluster)
    {
        // key vaults belong to the cluster
        var keyVaultRender = new KeyVaultRender()
        {
            AppName = appInfoDto.AppName,
            KeyVaultName = appInfoDto.KeyVault?.KeyVaultName!,
            ManagedId = appInfoDto.KeyVault?.ManagedId!,
            TenantId = appInfoDto.KeyVault?.TenantId!,
            ConfigKeyList = cluster.KeyVault?.Select(kv => kv.ConfigKey!).ToList() ?? new List<string>()
        };
    
        // key vaults belong to the whole application
        if (appInfoDto.KeyVault?.KeyVault != null)
        {
            keyVaultRender.ConfigKeyList.AddRange(
                appInfoDto.KeyVault.KeyVault
                    .Where(kv => kv.ConfigKey != null)  
                    .Select(kv => kv.ConfigKey!).ToList<string>()
            ); 
        }

        return keyVaultRender;
    }

    public async Task CreateClusterRoleAndBindingAsync(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var clusterRole = new V1ClusterRole
        {
            Metadata = new V1ObjectMeta
            {
                Name = "namespace-admin"
            },
            Rules = new[]
            {
                new V1PolicyRule
                {
                    ApiGroups = new[] { "" },
                    Resources = new[] { "namespaces", "services", "configmaps", "secrets", "pods", "nodes", "nodes/metrics", "nodes/proxy", "persistentvolumeclaims", "serviceaccounts" },
                    Verbs = new[] { "get", "list", "create", "delete", "update", "patch" }
                },
                new V1PolicyRule
                {
                    ApiGroups = new[] { "apps" },
                    Resources = new[] { "deployments", "statefulsets", "daemonsets", "replicasets" },
                    Verbs = new[] { "get", "list", "create", "delete", "update", "patch" }
                },
                new V1PolicyRule
                {
                    ApiGroups = new[] { "batch" },
                    Resources = new[] { "jobs", "cronjobs" },
                    Verbs = new[] { "get", "list", "create", "delete", "update", "patch" }
                },
                new V1PolicyRule
                {
                    ApiGroups = new[] { "extensions" },
                    Resources = new[] { "deployments", "replicasets", "ingresses" },
                    Verbs = new[] { "get", "list", "create", "delete", "update", "patch" }
                },
                new V1PolicyRule
                {
                    ApiGroups = new[] { "networking.k8s.io" },
                    Resources = new[] { "ingresses" },
                    Verbs = new[] { "get", "list", "create", "delete", "update", "patch" }
                },
                new V1PolicyRule
                {
                    ApiGroups = new[] { "rbac.authorization.k8s.io" },
                    Resources = new[] { "clusterroles", "clusterrolebindings" },
                    Verbs = new[] { "get", "list", "create", "delete", "update", "patch" }
                }
            }
        };

        try
        {
            await GetKubeClient(dto).CreateClusterRoleAsync(clusterRole, cancellationToken: cancellationToken);
            _logger.LogInformation("ClusterRole 'namespace-admin' created successfully.");
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogWarning("ClusterRole 'namespace-admin' already exists.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ClusterRole 'namespace-admin': {Message}", ex.Message);
            throw;
        }

        var clusterRoleBinding = new V1ClusterRoleBinding
        {
            Metadata = new V1ObjectMeta
            {
                Name = "namespace-admin-binding"
            },
            Subjects = new[]
            {
                new V1Subject
                {
                    Kind = "ServiceAccount",
                    Name = "yaml-helm-helm-yarl",
                    NamespaceProperty = "yaml"
                }
            },
            RoleRef = new V1RoleRef
            {
                ApiGroup = "rbac.authorization.k8s.io",
                Kind = "ClusterRole",
                Name = "namespace-admin"
            }
        };

        try
        {
            await GetKubeClient(dto).CreateClusterRoleBindingAsync(clusterRoleBinding, cancellationToken: cancellationToken);
            _logger.LogInformation("ClusterRoleBinding 'namespace-admin-binding' created successfully.");
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogWarning("ClusterRoleBinding 'namespace-admin-binding' already exists.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ClusterRoleBinding 'namespace-admin-binding': {Message}", ex.Message);
            throw;
        }
    }

    public async Task DeployNetaData(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {

        // create namespace
        CreateNs( dto, "netdata", cancellationToken);
        
        var yamlFilePath = Path.Combine(_currentDirectory, "YamlFile/yaml/netdata.yaml"); // 替换为你的 YAML 文件路径
        var kubectlManager = new KubectlManager();
        try
        {
            await kubectlManager.ApplyYamlAsync(yamlFilePath);
            _logger.LogInformation("YAML file applied successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to apply YAML file: {ex.Message}");
            throw;
        }
    }

    private async void CreateNs(YamlAppInfoDto dto, string namespaceName, CancellationToken cancellationToken)
    {
        // create namespace 
        var kubeClient = GetKubeClient(dto);
        try
        {
            await kubeClient.ReadNamespaceAsync(namespaceName, cancellationToken: cancellationToken);
        }
        catch (HttpOperationException ex) when(ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            // Namespace not found, create a new one
            _logger.LogInformation("Namespace {NamespaceName} not found. Creating new namespace", namespaceName);
            var newNamespace = new V1Namespace { Metadata = new V1ObjectMeta { Name = namespaceName } };
            await kubeClient.CreateNamespaceAsync(newNamespace, cancellationToken: cancellationToken);
            _logger.LogInformation("Namespace {NamespaceName} created successfully", namespaceName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating namespace '{NamespaceName}'", namespaceName);
            throw new ServiceException($"Create namespace error {namespaceName}", e);
        }
    }


    public async Task<V1Status> DeleteNamespace(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var kubeClient = GetKubeClient(dto);
        
        try
        {
            // 尝试删除命名空间
            _logger.LogInformation("Attempting to delete namespace {NamespaceName}", namespaceName);
            var deleteOptions = new V1DeleteOptions(); // 可以指定删除选项
            var deleteResult = await kubeClient.DeleteNamespaceAsync(namespaceName, deleteOptions, cancellationToken: cancellationToken);
            _logger.LogInformation("Namespace {NamespaceName} deleted successfully", namespaceName);
            return deleteResult;
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            // 如果命名空间不存在，则记录信息并可能抛出异常或返回null
            _logger.LogError("Namespace {NamespaceName} not found", namespaceName);
            throw new ServiceException($"Namespace {namespaceName} not found", ex);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting namespace '{NamespaceName}'", namespaceName);
            throw new ServiceException($"Delete namespace error {namespaceName}", e);
        }
    }

    public async Task DeleteService(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var kubeClient = GetKubeClient(dto);
        var serviceName = dto.AppName + KubeConstants.ServiceSuffix;
    
        try
        {
            _logger.LogInformation("Attempting to delete service {ServiceName} in namespace {NamespaceName}", serviceName, namespaceName);
            var deleteOptions = new V1DeleteOptions();
            await kubeClient.DeleteNamespacedServiceAsync(serviceName, namespaceName, deleteOptions, cancellationToken: cancellationToken);
            _logger.LogInformation("Service {ServiceName} deleted successfully", serviceName);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogError("Service {ServiceName} not found in namespace {NamespaceName}", serviceName, namespaceName);
            throw new ServiceException($"Service {serviceName} not found in namespace {namespaceName}", ex);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting service '{ServiceName}' in namespace '{NamespaceName}'", serviceName, namespaceName);
            throw new ServiceException($"Delete service error {serviceName} in namespace {namespaceName}", e);
        }
    }

    public async Task DeleteDeployment(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var kubeClient = GetKubeClient(dto);
        var deploymentName = dto.AppName + KubeConstants.DeploymentSuffix;
    
        try
        {
            _logger.LogInformation("Attempting to delete deployment {DeploymentName} in namespace {NamespaceName}", deploymentName, namespaceName);
            var deleteOptions = new V1DeleteOptions();
            await kubeClient.DeleteNamespacedDeploymentAsync(deploymentName, namespaceName, deleteOptions, cancellationToken: cancellationToken);
            _logger.LogInformation("Deployment {DeploymentName} deleted successfully", deploymentName);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogError("Deployment {DeploymentName} not found in namespace {NamespaceName}", deploymentName, namespaceName);
            throw new ServiceException($"Deployment {deploymentName} not found in namespace {namespaceName}", ex);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting deployment '{DeploymentName}' in namespace '{NamespaceName}'", deploymentName, namespaceName);
            throw new ServiceException($"Delete deployment error {deploymentName} in namespace {namespaceName}", e);
        }
    }

    public async Task DeleteConfigMap(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var kubeClient = GetKubeClient(dto);
        var configMapName = dto.AppName + KubeConstants.ConfigMapSuffix;
    
        try
        {
            _logger.LogInformation("Attempting to delete config map {ConfigMapName} in namespace {NamespaceName}", configMapName, namespaceName);
            var deleteOptions = new V1DeleteOptions();
            await kubeClient.DeleteNamespacedConfigMapAsync(configMapName, namespaceName, deleteOptions, cancellationToken: cancellationToken);
            _logger.LogInformation("Config map {ConfigMapName} deleted successfully", configMapName);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogError("Config map {ConfigMapName} not found in namespace {NamespaceName}", configMapName, namespaceName);
            throw new ServiceException($"Config map {configMapName} not found in namespace {namespaceName}", ex);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting config map '{ConfigMapName}' in namespace '{NamespaceName}'", configMapName, namespaceName);
            throw new ServiceException($"Delete config map error {configMapName} in namespace {namespaceName}", e);
        }
    }

    public async Task DeletePersistentVolume(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var kubeClient = GetKubeClient(dto);
        var ingressName = dto.AppName + KubeConstants.IngressSuffix;

        try
        {
            _logger.LogInformation("Attempting to delete ingress {IngressName} in namespace {NamespaceName}", ingressName, namespaceName);
            var deleteOptions = new V1DeleteOptions();
            await kubeClient.DeleteNamespacedIngressAsync(ingressName, namespaceName, deleteOptions, cancellationToken: cancellationToken);
            _logger.LogInformation("Ingress {IngressName} deleted successfully", ingressName);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogError("Ingress {IngressName} not found in namespace {NamespaceName}", ingressName, namespaceName);
            throw new ServiceException($"Ingress {ingressName} not found in namespace {namespaceName}", ex);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting ingress '{IngressName}' in namespace '{NamespaceName}'", ingressName, namespaceName);
            throw new ServiceException($"Delete ingress error {ingressName} in namespace {namespaceName}", e);
        }
    }

    public async Task DeletePersistentVolumeClaim(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var kubeClient = GetKubeClient(dto);
        var pvcName = dto.AppName + KubeConstants.PvcSubSuffix;

        try
        {
            _logger.LogInformation("Attempting to delete PVC {PvcName} in namespace {NamespaceName}", pvcName, namespaceName);
            var deleteOptions = new V1DeleteOptions();
            await kubeClient.DeleteNamespacedPersistentVolumeClaimAsync(pvcName, namespaceName, deleteOptions, cancellationToken: cancellationToken);
            _logger.LogInformation("PVC {PvcName} deleted successfully", pvcName);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("PVC {PvcName} not found in namespace {NamespaceName}", pvcName, namespaceName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting PVC '{PvcName}' in namespace '{NamespaceName}'", pvcName, namespaceName);
            throw new ServiceException($"Delete PVC error {pvcName} in namespace {namespaceName}", e);
        }
    }


    public async Task DeleteKeyVault(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var kubeClient = GetKubeClient(dto);
        var kvName = dto.AppName + "-keyvault-spc"; 
        try
        {
            _logger.LogInformation("Attempting to delete Key Vault {KeyVaultName} in namespace {NamespaceName}", kvName, namespaceName);
            var deleteOptions = new V1DeleteOptions();
            await kubeClient.DeleteNamespacedSecretAsync(kvName, namespaceName, deleteOptions, cancellationToken: cancellationToken);
            _logger.LogInformation("Key Vault {KeyVaultName} deleted successfully", kvName);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Key Vault {KeyVaultName} not found in namespace {NamespaceName}", kvName, namespaceName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting Key Vault '{KeyVaultName}' in namespace '{NamespaceName}'", kvName, namespaceName);
            throw new ServiceException($"Delete Key Vault error {kvName} in namespace {namespaceName}", e);
        }
    }


    public async Task DeleteDomainCertification(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var kubeClient = GetKubeClient(dto);
        List<Task<V1Status>> deleteTasks = new List<Task<V1Status>>();

        foreach (var cluster in dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
        {
            if (cluster.domainFlag) // Assuming domainFlag indicates the presence of a cert
            {
                var secretName = cluster.ClusterName + "-tls-secret";
                _logger.LogInformation("Attempting to delete certification secret {SecretName} in namespace {NamespaceName}", secretName, namespaceName);
                deleteTasks.Add(kubeClient.DeleteNamespacedSecretAsync(secretName, namespaceName, cancellationToken: cancellationToken));
            }
        }

        try
        {
            await Task.WhenAll(deleteTasks);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting domain certifications in namespace '{NamespaceName}'", namespaceName);
            throw new ServiceException("Error deleting domain certifications", e);
        }
    }


    public async Task DeleteClusterRoleAndBindingAsync(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var kubeClient = GetKubeClient(dto);
        var clusterRoleName = "namespace-admin"; // Adjust if you have specific naming conventions
        var clusterRoleBindingName = "namespace-admin-binding";

        try
        {
            _logger.LogInformation("Deleting ClusterRole {ClusterRoleName}", clusterRoleName);
            await kubeClient.DeleteClusterRoleAsync(clusterRoleName, cancellationToken: cancellationToken);
            _logger.LogInformation("ClusterRole {ClusterRoleName} deleted successfully", clusterRoleName);

            _logger.LogInformation("Deleting ClusterRoleBinding {ClusterRoleBindingName}", clusterRoleBindingName);
            await kubeClient.DeleteClusterRoleBindingAsync(clusterRoleBindingName, cancellationToken: cancellationToken);
            _logger.LogInformation("ClusterRoleBinding {ClusterRoleBindingName} deleted successfully", clusterRoleBindingName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting cluster role or binding");
            throw new ServiceException("Error deleting cluster role or binding", e);
        }
    }
    
    public async Task DeleteIngress(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var kubeClient = GetKubeClient(dto);
        var ingressName = dto.AppName + KubeConstants.IngressSuffix;

        try
        {
            _logger.LogInformation("Attempting to delete Ingress {IngressName} in namespace {NamespaceName}", ingressName, namespaceName);
            var deleteOptions = new V1DeleteOptions(); // Specify propagation policy if needed, e.g., OrphanDependents or Foreground
            await kubeClient.DeleteNamespacedIngressAsync(ingressName, namespaceName, deleteOptions, cancellationToken: cancellationToken);
            _logger.LogInformation("Ingress {IngressName} deleted successfully", ingressName);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Ingress {IngressName} not found in namespace {NamespaceName}", ingressName, namespaceName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting Ingress '{IngressName}' in namespace '{NamespaceName}'", ingressName, namespaceName);
            throw new ServiceException($"Delete Ingress error {ingressName} in namespace {namespaceName}", e);
        }
    }


    public Task DeleteNetaData(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}