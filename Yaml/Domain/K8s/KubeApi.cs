using System.Diagnostics;
using System.Net;
using System.Text;
using AutoMapper;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Caching.Memory;
using RazorLight;
using Yaml.Domain.K8s.Interface;
using Yaml.Infrastructure.CoustomService;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.Exception;
using Yaml.Infrastructure.YamlEum;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;

namespace Yaml.Domain.K8s;

public class KubeApi : IKubeApi
{
    private readonly IKuberYamlGenerator _yamlGenerator;
    private readonly IRazorLightEngine _engine;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly string _currentDirectory = Directory.GetCurrentDirectory();
    private readonly MyDbContext _context;
    public KubeApi(
        IKuberYamlGenerator yamlGenerator, 
        IRazorLightEngine engine, 
        ILogger<KubeApi> logger,
        IMemoryCache memoryCache,
        IMapper mapper, MyDbContext dbContext)
    {
        _yamlGenerator = yamlGenerator;
        _memoryCache = memoryCache;
        _engine = engine;
        _logger = logger;
        _mapper = mapper;
        _context = dbContext;;
    }

    private Kubernetes GetKubeClient(YamlAppInfoDto dto)
    {
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
            var configFile = Path.Combine(_currentDirectory, dto.KubeConfig);
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(@configFile);
            client = new Kubernetes(config);

            _memoryCache.Set(dto.Id, client);
        }
        catch (Exception ex)
        {
            throw new ServiceException("Error creating Kubernetes client.", ex);
        }
        return client ?? throw new ServiceException("Client creation failed unexpectedly.");
    }
    

    public async Task<V1Namespace> CreateNamespace(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        try
        {
            var kubeClient = GetKubeClient(dto);
            try
            {
                var existedNamespace = await kubeClient.ReadNamespaceAsync(namespaceName, cancellationToken: cancellationToken);
                return existedNamespace;
            }
            catch (HttpOperationException ex) when(ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                // Namespace not found - normal flow for creating a new namespace
                _logger.LogInformation(" Namespace not found - normal flow for creating a new namespace");
            }
            _logger.LogInformation("Creating new namespace: {NamespaceName}", namespaceName);
            var newNamespace = new V1Namespace {Metadata = new V1ObjectMeta { Name = namespaceName }};
            return await kubeClient.CreateNamespaceAsync(newNamespace, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating namespace '{NamespaceName}'", namespaceName);
            throw new ServiceException($"Create namespace error {namespaceName}", e);
        }
    }

    public async Task<V1Service[]> CreateService(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        try
        {
            var client = GetKubeClient(dto);
            var v1ServiceList = await client.ListNamespacedServiceAsync(namespaceName, cancellationToken: cancellationToken);
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
                .Select(async cluster =>
                {
                    var serviceName = cluster.ClusterName + KubeConstants.ServiceSuffix;
                    var service = v1ServiceList.Items.SingleOrDefault(service => service.Metadata.Name == serviceName);
                    if (service != null)
                    {
                        return service;
                    }

                    var content = await _yamlGenerator.GenerateService(cluster);
                    var v1Service = KubernetesYaml.Deserialize<V1Service>(content);
                    return await client.CreateNamespacedServiceAsync(v1Service, namespaceName, cancellationToken: cancellationToken);
                });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError("CreateConfigMap error [{}]", dto.AppName);
            _logger.LogError(e, "Error details: ");
            throw new ServiceException($"CreateConfigMap error {dto.AppName}", e);
        }
    }

    public async Task<V1Deployment[]> CreateDeployment(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        var path = Path.Combine(_currentDirectory, KubeConstants.DeploymentTemplate);
        try
        {
            var client = GetKubeClient(dto);
            var v1DeploymentList = await client.ListNamespacedDeploymentAsync(namespaceName, cancellationToken: cancellationToken);
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var deploymentName = cluster.ClusterName + KubeConstants.DeploymentSuffix;
                var v1Deployment = v1DeploymentList.Items.SingleOrDefault(v1Deployment => v1Deployment.Metadata.Name == deploymentName);
                if (v1Deployment != null)
                {
                    return v1Deployment;
                }

                cluster.AppName = dto.AppName;
                var content = await _engine.CompileRenderAsync(path, cluster);
                await File.WriteAllTextAsync(Path.Combine(_currentDirectory, KubeConstants.OutPutFile), content, cancellationToken);
                var v1Service = KubernetesYaml.Deserialize<V1Deployment>(content);
                return await client.CreateNamespacedDeploymentAsync(v1Service, namespaceName, cancellationToken: cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError("CreateDeployment error [{}]", dto.AppName);
            _logger.LogError(e, "Error details: ");
            throw new ServiceException($"CreateDeployment error {dto.AppName}", e);
        }
    }

    public async Task<V1ConfigMap?[]> CreateConfigMap(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.ConfigMapTemplate);
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;

        try
        {
            var client = GetKubeClient(dto);
            var v1ConfigMapList = await client.ListNamespacedConfigMapAsync(namespaceName, cancellationToken: cancellationToken);
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var configMapName = cluster.ClusterName + KubeConstants.ConfigMapSuffix;
                var configMap = v1ConfigMapList.Items.SingleOrDefault(cf => cf.Metadata.Name == configMapName);
                if (configMap != null)
                {
                    return configMap;
                }

                cluster.AppName = dto.AppName;
                var content = await _engine.CompileRenderAsync(path, cluster);
                var v1ConfigMap = KubernetesYaml.Deserialize<V1ConfigMap>(content);
                return await client.CreateNamespacedConfigMapAsync(v1ConfigMap, namespaceName, cancellationToken: cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("CreateConfigMap error [{}]", dto.AppName);
            _logger.LogError(ex, "Error details: ");
            throw new ServiceException($"CreateConfigMap error {dto.AppName}", ex);
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
                            // prepare the parameter
                            var diskInfo = cluster.DiskInfoList[i];
                            var storage = diskInfo.DiskSize;
                            var storageClassName = diskInfo.DiskType;
                            var subscriptionId = dto.KeyVault?.ManagedId;
                            var resourceGroup = "saas-core"; // TODO where should I get resourceGroup
                        
                            // generate pv content
                            var content = await _yamlGenerator.GeneratePersistentVolume(
                                pvName, 
                                storage, storageClassName,
                                subscriptionId, resourceGroup
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

    public async Task<List<V1PersistentVolumeClaim>> CreatePersistentVolumeClaim(
        YamlAppInfoDto dto,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.PersistentVolumeClaimTemplate);
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        try
        {
            var resList = new List<V1PersistentVolumeClaim>();

            var client = GetKubeClient(dto);
            var pvsList = await client.ListNamespacedPersistentVolumeClaimAsync(namespaceName, cancellationToken: cancellationToken);
            foreach(var cluster in dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
            {
                
                for (var i = 0; i < cluster.DiskInfoList?.Count; i++)
                {
                    var diskInfoDto = cluster.DiskInfoList[i];
                    diskInfoDto.AppName = dto.AppName;
                    var persistentVolumeClaim = pvsList.Items.SingleOrDefault(pvc => pvc.Metadata.Name == diskInfoDto.Name + KubeConstants.PvcSubSuffix);
                    // when pvc is not found, then create a new pvc
                    if (persistentVolumeClaim == null)
                    {
                        var content = await _engine.CompileRenderAsync(path, diskInfoDto);
                        var pvc = KubernetesYaml.Deserialize<V1PersistentVolumeClaim>(content);
                        var pvcClaim = await client.CreateNamespacedPersistentVolumeClaimAsync(pvc, namespaceName, cancellationToken: cancellationToken);
                        resList.Add(pvcClaim);
                        _logger.LogInformation("deploy pvc {}", pvcClaim.Metadata.Name);
                    }
                }
            }
            return resList;
        }
        catch (Exception ex)
        {
            _logger.LogError("Create V1PersistentVolumeClaim  [{}] Error", dto.AppName);
            _logger.LogError(ex, "Error details: ");
            throw new ServiceException($"Create V1PersistentVolumeClaim error {dto.AppName}", ex);
        }
    }

    public async Task<string> CreateKeyVault(YamlAppInfoDto appInfoDto, CancellationToken cancellationToken)
    {

        var client = GetKubeClient(appInfoDto);
        foreach (var cluster in appInfoDto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
        {
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
            
            var secretObjects = new List<object>();
            var objects = "";
            foreach (var keyVault in keyVaultRender.ConfigKeyList ?? new List<string>())
            {
                secretObjects.Add(new Dictionary<string, string>
                    {
                        ["key"] = keyVault,
                        ["objectName"] = keyVault
                    }
                );
                objects +=
                    $"        - | \n" +
                    $"          objectType: secret \n" +
                    $"          objectName: {keyVault} \n";
            }
            
            if(CloudType.AWS == appInfoDto.CloudType)
            {
                // 创建 SecretProviderClass 的动态对象
                var secretProviderClass = new Unstructured
                {
                    ApiVersion = "secrets-store.csi.x-k8s.io/v1",
                    Kind = "SecretProviderClass",
                    Metadata = new V1ObjectMeta
                    {
                        Name = $"{appInfoDto.AppName}-keyvault-spc",
                        NamespaceProperty = $"{appInfoDto.AppName}-ns"
                    },

                    Spec = new Dictionary<string, object>
                    {
                        ["provider"] = "aws",
                        ["parameters"] = new Dictionary<string, object>
                        {
                            // ["region"] = "ap-northeast-1",
                            ["keyvaultName"] = keyVaultRender.KeyVaultName,
                            ["objects"] = $"\n{objects}"
                        },
                        // ["secretObjects"] = new List<object>
                        // {
                        //     new Dictionary<string, object>
                        //     {
                        //         ["secretName"] = $"{appInfoDto.AppName}-secret",
                        //         ["type"] = "Opaque",
                        //         ["data"] = secretObjects
                        //     }
                        // }
                    }
                };
                // 创建 SecretProviderClass 资源
                var result = client.CreateNamespacedCustomObject(secretProviderClass, "secrets-store.csi.x-k8s.io", "v1", secretProviderClass.Metadata.NamespaceProperty, "secretproviderclasses");
                Console.WriteLine($"Created SecretProviderClass: {result}");
                return "success";
            }
            if (CloudType.Azure == appInfoDto.CloudType)
            {
                var secretProviderClass = new Unstructured
                {
                    ApiVersion = "secrets-store.csi.x-k8s.io/v1",
                    Kind = "SecretProviderClass",
                    Metadata = new V1ObjectMeta
                    {
                        Name = $"{appInfoDto.AppName}-keyvault-spc",
                        NamespaceProperty = $"{appInfoDto.AppName}-ns"
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
                // 创建一个序列化器
                var serializer = new SerializerBuilder()
                    .WithTypeInspector(inner => new ReadablePropertiesTypeInspector(new DynamicTypeResolver())) // 使输出更友好
                    .Build();

                var yaml = serializer.Serialize(secretProviderClass);
                Console.WriteLine(yaml);
                var result = client.CreateNamespacedCustomObject(secretProviderClass, "secrets-store.csi.x-k8s.io", "v1", secretProviderClass.Metadata.NamespaceProperty, "secretproviderclasses");
                Console.WriteLine($"Created SecretProviderClass: {result}");
                return "";
            }
        }

        return "success";
    }
    
    
    

    public async Task<V1Ingress[]> CreateIngress(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.IngressFileTemplate);
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        try
        {
            var client = GetKubeClient(dto);
            var ingressList = 
                await client.ListNamespacedIngressAsync(namespaceName, cancellationToken: cancellationToken);
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var pvcName = cluster.ClusterName + KubeConstants.IngressSuffix;
                var ingressSig = ingressList.Items.SingleOrDefault(pvc => pvc.Metadata.Name == pvcName);
                if (ingressSig != null)
                {
                    return ingressSig;
                }

                var content = await _engine.CompileRenderAsync(path, cluster);
                var ingress = KubernetesYaml.Deserialize<V1Ingress>(content);
                return await client.CreateNamespacedIngressAsync(ingress, namespaceName, cancellationToken: cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("Create Ingress  [{}] Error", dto.AppName);
            throw new ServiceException($"Create Ingress error {dto.AppName}", ex);
        }
    }

    public async Task<V1Secret[]> CreateDomainCertification(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        try
        {    
            var client = GetKubeClient(dto);
            var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
            var tasks = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var secretName = cluster.ClusterName + "-tls-secret";
                try
                {
                    // Optionally skip this step if secrets are not expected to exist beforehand
                    return await client.ReadNamespacedSecretAsync(secretName, namespaceName, cancellationToken: cancellationToken);
                }
                catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    var certificationLocation = Path.Combine(_currentDirectory, cluster.Domain?.Certification ?? "");
                    var privateKeyLocation = Path.Combine(_currentDirectory, cluster.Domain?.PrivateKey ?? "");

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
}