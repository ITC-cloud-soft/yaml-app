using System.Net;
using System.Text;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Azure.Management.Msi.Fluent;
using RazorLight;
using Yaml.Domain.AzureApi.Interface;
using Yaml.Domain.K8s.Interface;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.Exception;

namespace Yaml.Domain.K8s;

public class KubeApi : IKubeApi
{
    private readonly IAzureIdentityManager _azureIdentityManager;
    private readonly IKuberYamlGenerator _yamlGenerator;
    private readonly IRazorLightEngine _engine;
    private readonly Kubernetes _client;
    private readonly ILogger _logger;
    private readonly string _currentDirectory = Directory.GetCurrentDirectory();

    public KubeApi(
        IAzureIdentityManager azureIdentityManager,
        IKuberYamlGenerator yamlGenerator, 
        IRazorLightEngine engine, 
        Kubernetes client,  
        ILogger<KubeApi> logger)
    {
        _azureIdentityManager = azureIdentityManager;
        _yamlGenerator = yamlGenerator;
        _engine = engine;
        _client = client;
        _logger = logger;
    }

    public async Task<V1Namespace> CreateNamespace(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        try
        {
            try
            {
                var existedNamespace = await _client.ReadNamespaceAsync(namespaceName, cancellationToken: cancellationToken);
                return existedNamespace;
            }
            catch (HttpOperationException ex) when(ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                // Namespace not found - normal flow for creating a new namespace
            }
            _logger.LogInformation("Creating new namespace: {NamespaceName}", namespaceName);
            var newNamespace = new V1Namespace {Metadata = new V1ObjectMeta { Name = namespaceName }};
            return await _client.CreateNamespaceAsync(newNamespace, cancellationToken: cancellationToken);
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
            var v1ServiceList = await _client.ListNamespacedServiceAsync(namespaceName, cancellationToken: cancellationToken);
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
                    return await _client.CreateNamespacedServiceAsync(v1Service, namespaceName, cancellationToken: cancellationToken);
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
            var v1DeploymentList = await _client.ListNamespacedDeploymentAsync(namespaceName, cancellationToken: cancellationToken);
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var deploymentName = cluster.ClusterName + KubeConstants.DeploymentSuffix;
                var v1Deployment = v1DeploymentList.Items.SingleOrDefault(v1Deployment => v1Deployment.Metadata.Name == deploymentName);
                if (v1Deployment != null)
                {
                    return v1Deployment;
                }

                var content = await _engine.CompileRenderAsync(path, cluster);
                await File.WriteAllTextAsync(Path.Combine(_currentDirectory, KubeConstants.OutPutFile), content, cancellationToken);
                var v1Service = KubernetesYaml.Deserialize<V1Deployment>(content);
                return await _client.CreateNamespacedDeploymentAsync(v1Service, namespaceName, cancellationToken: cancellationToken);
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
            var v1ConfigMapList = await _client.ListNamespacedConfigMapAsync(namespaceName, cancellationToken: cancellationToken);
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var configMapName = cluster.ClusterName + KubeConstants.ConfigMapSuffix;
                var configMap = v1ConfigMapList.Items.SingleOrDefault(cf => cf.Metadata.Name == configMapName);
                if (configMap != null)
                {
                    return configMap;
                }

                var content = await _engine.CompileRenderAsync(path, cluster);
                var v1ConfigMap = KubernetesYaml.Deserialize<V1ConfigMap>(content);
                return await _client.CreateNamespacedConfigMapAsync(v1ConfigMap, namespaceName, cancellationToken: cancellationToken);
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

    public Task<V1PersistentVolume> CreatePersistentVolume(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        // TODO CreatePersistentVolume
        throw new NotImplementedException();
    }

    public async Task<V1PersistentVolumeClaim[]> CreatePersistentVolumeClaim(YamlAppInfoDto dto,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.PersistentVolumeClaimTemplate);
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        try
        {
            var pvsList = await _client.ListNamespacedPersistentVolumeClaimAsync(namespaceName, cancellationToken: cancellationToken);
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var pvcName = cluster.ClusterName + KubeConstants.PvcSubSuffix;
                var persistentVolumeClaim = pvsList.Items.SingleOrDefault(pvc => pvc.Metadata.Name == pvcName);
                if (persistentVolumeClaim != null)
                {
                    return persistentVolumeClaim;
                }

                var content = await _engine.CompileRenderAsync(path, cluster);
                var pvc = KubernetesYaml.Deserialize<V1PersistentVolumeClaim>(content);
                return await _client.CreateNamespacedPersistentVolumeClaimAsync(pvc, namespaceName, cancellationToken: cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("Create V1PersistentVolumeClaim  [{}] Error", dto.AppName);
            _logger.LogError(ex, "Error details: ");
            throw new ServiceException($"Create V1PersistentVolumeClaim error {dto.AppName}", ex);
        }
    }

    public async Task<V1Secret> CreateKeyVault(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.SecretTemplate);
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        try
        {
            var secretList =
                await _client.ListNamespacedSecretAsync(namespaceName, cancellationToken: cancellationToken);
            var secretName = dto.AppName + KubeConstants.SecretSuffix;
            var secretSig = secretList.Items.SingleOrDefault(pvc => pvc.Metadata.Name == secretName);
            if (secretSig != null)
            {
                return secretSig;
            }

            var content = await _engine.CompileRenderAsync(path, dto);
            await File.WriteAllTextAsync(Path.Combine(_currentDirectory, KubeConstants.OutPutFile), content, cancellationToken);

            var secret = KubernetesYaml.Deserialize<V1Secret>(content);
            return await _client.CreateNamespacedSecretAsync(secret, namespaceName, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("Create Secret  [{}] Error", dto.AppName);
            _logger.LogError(ex, "Error details: ");
            throw new ServiceException($"Create Secret error {dto.AppName}", ex);
        }
    }

    public async Task<V1Ingress[]> CreateIngress(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.IngressFileTemplate);
        var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;
        try
        {
            var ingressList =
                await _client.ListNamespacedIngressAsync(namespaceName, cancellationToken: cancellationToken);
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
                return await _client.CreateNamespacedIngressAsync(ingress, namespaceName, cancellationToken: cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("Create Ingress  [{}] Error", dto.AppName);
            _logger.LogError(ex, "Error details: ");
            throw new ServiceException($"Create Ingress error {dto.AppName}", ex);
        }
    }

    public async Task<V1Secret[]> CreateDomainCertification(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var namespaceName = dto.AppName + KubeConstants.NamespaceSuffix;

            var tasks = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var secretName = cluster.ClusterName + "-tls-secret";
                try
                {
                    // Optionally skip this step if secrets are not expected to exist beforehand
                    return await _client.ReadNamespacedSecretAsync(secretName, namespaceName, cancellationToken: cancellationToken);
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
                    return await _client.CreateNamespacedSecretAsync(
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

    public async Task CreateAzureIdentityAsync(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var tasks = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var azureIdentityName = cluster.ClusterName + "-identity";
                var identityAsync = await _azureIdentityManager.CreateIdentityAsync(azureIdentityName, "resourceGroupName");
                return identityAsync;
            });
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Create Azure Identity Async for namespace: {AppName}", dto.AppName);
            throw new ServiceException("Error Create Azure Identity Async for namespace", e);
        }
    }

    public async Task CreateAzureIdentityBindingAsync(string namespaceName, string bindingName, string selector,  IIdentity identity)
    {
        
        var azureIdentityBinding = new Dictionary<string, object>
        {
            ["apiVersion"] = "aadpodidentity.k8s.io/v1",
            ["kind"] = "AzureIdentityBinding",
            ["metadata"] = new Dictionary<string, object>
            {
                ["name"] = bindingName,
                ["namespace"] = namespaceName
            },
            ["spec"] = new Dictionary<string, object>
            {
                ["azureIdentity"] = identity.Name,
                ["selector"] = selector
            }
        };

        await _client.CreateNamespacedCustomObjectAsync(azureIdentityBinding,
            "aadpodidentity.k8s.io", 
            "v1",
            namespaceName, 
            "azureidentitybindings");
    }
}