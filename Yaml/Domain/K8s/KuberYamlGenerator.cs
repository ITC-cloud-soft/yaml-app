using RazorLight;
using Yaml.Domain.K8s.Interface;
using Yaml.Infrastructure.Dto;

namespace Yaml.Domain.K8s;

public class KuberYamlGenerator : IKuberYamlGenerator 
{
    
    private readonly IRazorLightEngine _engine;
    private readonly ILogger _logger;
    private readonly string _currentDirectory = Directory.GetCurrentDirectory();

    public KuberYamlGenerator(IRazorLightEngine engine, ILogger<KuberYamlGenerator> logger)
    {
        _engine = engine;
        _logger = logger;
    }
 
    public async Task<string> GenerateConfigMap(YamlClusterInfoDto cluster)
    {  
        var path = Path.Combine(_currentDirectory, KubeConstants.ConfigMapTemplate);
        return await _engine.CompileRenderAsync(path, cluster);
    }

    public async Task<string> GenerateService(YamlClusterInfoDto cluster)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.ServiceTemplate);
        return await _engine.CompileRenderAsync(path, cluster);
    }

    public async Task<string> GenerateDeployment(YamlClusterInfoDto cluster)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.DeploymentTemplate);
        return await _engine.CompileRenderAsync(path, cluster);
    }

    public async Task<string> GeneratePersistentVolume(YamlClusterInfoDto cluster)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.PersistentVolumeTemplate);
        return await _engine.CompileRenderAsync(path, cluster);
    }

    public async Task<string> GeneratePersistentVolumeClaim(YamlClusterInfoDto cluster)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.PersistentVolumeClaimTemplate);
        return await _engine.CompileRenderAsync(path, cluster);
    }

    public async Task<string> GenerateSecret(YamlAppInfoDto appInfoDto)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.SecretTemplate);
        return await _engine.CompileRenderAsync(path, appInfoDto);
    }

    public async Task<string> GenerateIngress(YamlClusterInfoDto cluster)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.IngressFileTemplate);
        return await _engine.CompileRenderAsync(path, cluster);
    }
}