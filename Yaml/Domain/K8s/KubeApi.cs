using System.Net;
using k8s;
using k8s.Autorest;
using k8s.Exceptions;
using k8s.Models;
using RazorLight;
using Yaml.Domain.K8s.Interface;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.Exception;

namespace Yaml.Domain.K8s;

public class KubeApi : IKubeApi
{
    private readonly IKuberYamlGenerator _yamlGenerator;
    private readonly IRazorLightEngine _engine;
    private readonly Kubernetes _client;
    private readonly ILogger _logger;
    private readonly string _currentDirectory = Directory.GetCurrentDirectory();

    public KubeApi(IKuberYamlGenerator yamlGenerator, IRazorLightEngine engine, Kubernetes client, ILogger<KubeApi> logger)
    {
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

    public async Task<V1Secret> CreateSecret(YamlAppInfoDto dto, CancellationToken cancellationToken)
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
            Console.WriteLine(content);
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
}