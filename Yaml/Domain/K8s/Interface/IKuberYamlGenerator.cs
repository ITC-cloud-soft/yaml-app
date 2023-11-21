using Yaml.Infrastructure.Dto;

namespace Yaml.Domain.K8s.Interface;

public interface IKuberYamlGenerator
{

    public string GenerateService(YamlClusterInfoDto cluster);
    
    public Task<string> GenerateConfigMap(YamlClusterInfoDto cluster);

}