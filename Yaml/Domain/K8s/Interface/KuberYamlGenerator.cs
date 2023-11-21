using RazorLight;
using Yaml.Infrastructure.Dto;

namespace Yaml.Domain.K8s.Interface;

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

    public string GenerateService(YamlClusterInfoDto cluster)
    {
        throw new NotImplementedException();
    }

    public Task<string> GenerateConfigMap(YamlClusterInfoDto cluster)
    {  
        var path = Path.Combine(_currentDirectory, KubeConstants.ConfigMapTemplate);
        return _engine.CompileRenderAsync(path, cluster);
    }
}