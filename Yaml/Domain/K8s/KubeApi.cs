using k8s;
using k8s.Models;
using RazorLight;
using Yaml.Domain.K8s.Interface;
using Yaml.Infrastructure.Dto;
namespace Yaml.Domain.K8s;

public class KubeApi : IKubeApi
{
    private readonly  IRazorLightEngine _engine;
    private readonly  Kubernetes _client;
    private readonly ILogger _logger;
    private readonly string _currentDirectory = Directory.GetCurrentDirectory();

    private const string ConfigMapTemplate = "YamlFile/ConfigMap.cshtml";
    private const string PersistentVolumeClaimTemplate = "YamlFile/PersistentVolumeClaim.cshtml";
    private const string DeploymentTemplate = "YamlFile/Deployment.cshtml";
    private const string ServiceTemplate = "YamlFile/Service.cshtml";
    private const string SecretTemplate = "YamlFile/Keyvault.cshtml";
    private const string OutPutFile = "YamlFile/App.yaml";
    private const string IngressFile = "YamlFile/Ingress.cshtml";
    
    private const string NamespaceSuffix = "-ns";
    private const string PvcSubSuffix = "-pvc";
    private const string ConfigMapSuffix  = "-configmap";
    private const string ServiceSuffix  = "-svc";
    private const string DeploymentSuffix  = "-deployment";
    private const string SecretSuffix  = "-secret";
    private const string IngressSuffix  = "-ingress";
    
    public KubeApi(IRazorLightEngine engine, Kubernetes client, ILogger<KubeApi> logger)
    {
        _engine = engine;
        _client = client;
        _logger = logger;
    }

    // await File.WriteAllTextAsync(Path.Combine(currentDirectory, OutPutFile), content, cancellationToken);
    
    public async Task<V1Namespace> CreateNamespace(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + NamespaceSuffix;
        try
        {
            // check namespace if exists
            var namespaces = await _client.ListNamespaceAsync(cancellationToken:cancellationToken);
            var namespaceNs = namespaces.Items.SingleOrDefault(ns => ns.Metadata.Name == namespaceName);

            if (namespaceNs != null)
            {
                return namespaceNs;
            }
            
            var newNamespace = new V1Namespace
            {
                Metadata = new V1ObjectMeta { Name = namespaceName }
            };

            var namespaceAsync =await _client.CreateNamespaceAsync(newNamespace, cancellationToken: cancellationToken);
            return namespaceAsync;
        }
        catch (Exception e)
        {
            _logger.LogError("Create namespace error [{}]", namespaceName);
            _logger.LogError(e, "Error details: ");
            throw e;
        }
    }

    public async Task<V1Service[]> CreateService(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, ServiceTemplate);
        var namespaceName = dto.AppName + NamespaceSuffix;
        try
        {
            var v1ServiceList = await _client.ListNamespacedServiceAsync(namespaceName, cancellationToken: cancellationToken);
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var serviceName = cluster.ClusterName + ServiceSuffix;
                var service = v1ServiceList.Items.SingleOrDefault(service => service.Metadata.Name == serviceName);
                if (service != null)
                {
                    return service;
                }
                var content = await _engine.CompileRenderAsync(path, cluster);
                var v1Service = KubernetesYaml.Deserialize<V1Service>(content);
                return await _client.CreateNamespacedServiceAsync(v1Service, namespaceName, cancellationToken:cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError("CreateConfigMap error [{}]", dto.AppName);
            _logger.LogError(e, "Error details: ");
            throw e;
        }
    }

    public async Task<V1Deployment[]> CreateDeployment(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var namespaceName = dto.AppName + NamespaceSuffix;
        var path = Path.Combine(_currentDirectory, DeploymentTemplate);
        try
        {
            var v1DeploymentList = await _client.ListNamespacedDeploymentAsync(namespaceName, cancellationToken: cancellationToken);
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var deploymentName = cluster.ClusterName + DeploymentSuffix;
                var v1Deployment = v1DeploymentList.Items.SingleOrDefault(v1Deployment => v1Deployment.Metadata.Name == deploymentName);
                if (v1Deployment != null)
                {
                    return v1Deployment;
                }
                var content = await _engine.CompileRenderAsync(path, cluster);
                await File.WriteAllTextAsync(Path.Combine(_currentDirectory, OutPutFile), content, cancellationToken);
                var v1Service = KubernetesYaml.Deserialize<V1Deployment>(content);
                return await _client.CreateNamespacedDeploymentAsync(v1Service, namespaceName, cancellationToken:cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError("CreateDeployment error [{}]", dto.AppName);
            _logger.LogError(e, "Error details: ");
            throw e;
        }
    }

    public async Task<V1ConfigMap?[]> CreateConfigMap(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, ConfigMapTemplate);
        var namespaceName = dto.AppName + NamespaceSuffix;
    
        try
        {
            var v1ConfigMapList = await _client.ListNamespacedConfigMapAsync(namespaceName, cancellationToken: cancellationToken);
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var configMapName = cluster.ClusterName + ConfigMapSuffix;
                var configMap = v1ConfigMapList.Items.SingleOrDefault(cf => cf.Metadata.Name == configMapName);
                if (configMap != null)
                {
                    return configMap;
                }
                var content = await _engine.CompileRenderAsync(path, cluster);
                var v1ConfigMap = KubernetesYaml.Deserialize<V1ConfigMap>(content);
                return await _client.CreateNamespacedConfigMapAsync(v1ConfigMap, namespaceName, cancellationToken:cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("CreateConfigMap error [{}]", dto.AppName);
            _logger.LogError(ex, "Error details: ");
            throw ex;
        }
    }

    public Task<V1PersistentVolume> CreatePersistentVolume(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    public async Task<V1PersistentVolumeClaim[]> CreatePersistentVolumeClaim(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, PersistentVolumeClaimTemplate);
        var namespaceName = dto.AppName + NamespaceSuffix;
        try
        {
            var pvsList = await _client.ListNamespacedPersistentVolumeClaimAsync(namespaceName, cancellationToken: cancellationToken);
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var pvcName = cluster.ClusterName + PvcSubSuffix;
                var persistentVolumeClaim = pvsList.Items.SingleOrDefault(pvc => pvc.Metadata.Name == pvcName);
                if (persistentVolumeClaim != null)
                {
                    return persistentVolumeClaim;
                }
                var content = await _engine.CompileRenderAsync(path, cluster);
                var pvc = KubernetesYaml.Deserialize<V1PersistentVolumeClaim>(content);
                return await _client.CreateNamespacedPersistentVolumeClaimAsync(pvc, namespaceName, cancellationToken:cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("Create V1PersistentVolumeClaim  [{}] Error", dto.AppName);
            _logger.LogError(ex, "Error details: ");
            throw ex;
        }
    }

    public async Task<V1Secret[]> CreateSecret(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, SecretTemplate);
        var namespaceName = dto.AppName + NamespaceSuffix;
        try
        {
            var secretList = await _client.ListNamespacedSecretAsync(namespaceName, cancellationToken: cancellationToken);
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var secretName = cluster.ClusterName + SecretSuffix;
                var secretSig = secretList.Items.SingleOrDefault(pvc => pvc.Metadata.Name == secretName);
                if (secretSig != null)
                {
                    return secretSig;
                }
                var content = await _engine.CompileRenderAsync(path, cluster);
                var secret = KubernetesYaml.Deserialize<V1Secret>(content);
                return await _client.CreateNamespacedSecretAsync(secret, namespaceName, cancellationToken:cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("Create Secret  [{}] Error", dto.AppName);
            _logger.LogError(ex, "Error details: ");
            throw ex;
        }
    }
    
    public async Task<V1Ingress[]> CreateIngress(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, IngressFile);
        var namespaceName = dto.AppName + NamespaceSuffix;
        try
        {
            var ingressList = await _client.ListNamespacedIngressAsync(namespaceName, cancellationToken: cancellationToken);
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                var pvcName = cluster.ClusterName + IngressSuffix;
                var ingressSig = ingressList.Items.SingleOrDefault(pvc => pvc.Metadata.Name == pvcName);
                if (ingressSig != null)
                {
                    return ingressSig;
                }
                var content = await _engine.CompileRenderAsync(path, cluster);
                var ingress = KubernetesYaml.Deserialize<V1Ingress>(content);
                return await _client.CreateNamespacedIngressAsync(ingress, namespaceName, cancellationToken:cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("Create Secret  [{}] Error", dto.AppName);
            _logger.LogError(ex, "Error details: ");
            throw ex;
        }
    }
}