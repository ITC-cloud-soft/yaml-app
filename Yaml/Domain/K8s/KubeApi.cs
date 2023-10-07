using k8s;
using k8s.Models;
using RazorLight;
using Yaml.Domain.K8s.Interface;
using Yaml.Infrastructure.Dto;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Yaml.Domain.K8s;

public class KubeApi : IKubeApi
{
    private readonly  IRazorLightEngine _engine;
    private readonly  Kubernetes _client;
    private readonly ILogger _logger;

    private const string ConfigMapTemplate = "YamlFile/ConfigMap.cshtml";
    private const string PersistentVolumeClaimTemplate = "YamlFile/PersistentVolumeClaim.cshtml";
    private const string DeploymentTemplate = "YamlFile/Deployment.cshtml";
    private const string ServiceTemplate = "YamlFile/Service.cshtml";
    
    private const string OutPutFile = "YamlFile/App.yaml";
    private string _currentDirectory = Directory.GetCurrentDirectory();
    public KubeApi(IRazorLightEngine engine, Kubernetes client, ILogger<KubeApi> logger)
    {
        _engine = engine;
        _client = client;
        _logger = logger;
    }

    //  Console.WriteLine(content);
    // await File.WriteAllTextAsync(Path.Combine(currentDirectory, OutPutFile), content, cancellationToken);
    
    public Task<V1Namespace> CreateNamespace(string namespaceName, CancellationToken cancellationToken)
    {
 
        // if not exist
        var namespaceAsync = _client.CreateNamespaceAsync(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = namespaceName }
        }, cancellationToken: cancellationToken);
        return namespaceAsync;
    }

    public async Task<V1Service[]> CreateService(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, ServiceTemplate);
        try
        {
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                // render cshtml to yaml string form 
                var content = await _engine.CompileRenderAsync(path, cluster);
                var v1Service = KubernetesYaml.Deserialize<V1Service>(content);
                return await _client.CreateNamespacedServiceAsync(v1Service, cluster.AppName, cancellationToken:cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError("CreateConfigMap error [{}]", dto.AppName);
            _logger.LogError(e, "Error details: ");
            throw new Exception(e.Message);
        }
    }

    public Task<V1Deployment> CreateDeployment(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var path = Path.Combine(currentDirectory, DeploymentTemplate);
        var content =  _engine.CompileRenderAsync(path, dto);
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        // 解析 YAML 内容为 Kubernetes 对象
        var deployment = deserializer.Deserialize<V1Deployment>(new StringReader(""));

        return  _client.CreateNamespacedDeploymentAsync(deployment, dto.AppName, cancellationToken:cancellationToken);
    }

    /// <summary>
    /// YamlAppInfoDtoを使用して新しいConfigMapを作成します。
    /// </summary>
    /// <param name="dto">YamlAppInfoDtoの情報</param>
    /// <param name="cancellationToken">操作をキャンセルするためのトークン</param>
    /// <returns>作成されたConfigMapの情報の配列</returns>
    public async Task<V1ConfigMap[]> CreateConfigMap(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, ConfigMapTemplate);
        try
        {
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                // render cshtml to yaml string form 
                var content = await _engine.CompileRenderAsync(path, cluster);
                var v1ConfigMap = KubernetesYaml.Deserialize<V1ConfigMap>(content);
                return await _client.CreateNamespacedConfigMapAsync(v1ConfigMap, cluster.AppName, cancellationToken:cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError("CreateConfigMap error [{}]", dto.AppName);
            _logger.LogError(e, "Error details: ");
            throw new Exception(e.Message);
        }
    }

    public Task<V1PersistentVolume> CreatePersistentVolume(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    

    /// <summary>
    /// YamlAppInfoDtoを使用して新しい永続ボリュームクレームを作成します。
    /// </summary>
    /// <param name="dto">YamlAppInfoDtoの情報</param>
    /// <param name="cancellationToken">操作をキャンセルするためのトークン</param>
    /// <returns>作成された永続ボリュームクレームの情報の配列</returns>
    public async Task<V1PersistentVolumeClaim[]> CreatePersistentVolumeClaim(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_currentDirectory, PersistentVolumeClaimTemplate);
        try
        {
            var task = (dto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>()).Select(async cluster =>
            {
                // render cshtml to yaml string form 
                var content = await _engine.CompileRenderAsync(path, cluster);
                Console.WriteLine(content);
                var pvc = KubernetesYaml.Deserialize<V1PersistentVolumeClaim>(content);
                return await _client.CreateNamespacedPersistentVolumeClaimAsync(pvc, cluster.AppName, cancellationToken:cancellationToken);
            });
            return await Task.WhenAll(task).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError("Create V1PersistentVolumeClaim  [{}] Error", dto.AppName);
            _logger.LogError(e, "Error details: ");
            throw new Exception(e.Message);
        }
    }
}