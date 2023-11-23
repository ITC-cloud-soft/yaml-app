using Yaml.Infrastructure.Dto;

namespace Yaml.Domain.K8s.Interface;

public interface IKuberYamlGenerator
{

    public Task<string> GenerateService(YamlClusterInfoDto cluster);
    
    public Task<string> GenerateDeployment(YamlClusterInfoDto cluster);
    
    public Task<string> GenerateConfigMap(YamlClusterInfoDto cluster);
  
    public Task<string> GeneratePersistentVolume(YamlClusterInfoDto cluster);
    
    public Task<string> GeneratePersistentVolumeClaim(YamlClusterInfoDto cluster);
    
    public Task<string> GenerateSecret(YamlAppInfoDto cluster);
    
    public Task<string> GenerateIngress(YamlClusterInfoDto cluster);

}