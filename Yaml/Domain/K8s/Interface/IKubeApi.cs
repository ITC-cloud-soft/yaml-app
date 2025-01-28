using k8s.Models;
using Yaml.Infrastructure.Dto;

namespace Yaml.Domain.K8s.Interface;
public interface IKubeApi
{
    public Task<V1Namespace> CreateNamespace(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task CreateService(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task CreateDeployment(YamlAppInfoDto dto, CancellationToken cancellationToken);
   
    public Task CreateConfigMap(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task CreateConfigMapFile(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task<List<V1PersistentVolume>> CreatePersistentVolume(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task CreatePersistentVolumeClaim(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task CreateKeyVault(YamlAppInfoDto appInfoDto, CancellationToken cancellationToken);
    
    public Task CreateIngress(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task<V1Secret[]> CreateDomainCertification(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task CreateClusterRoleAndBindingAsync(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task DeployNetaData(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    
    public Task<V1Status> DeleteNamespace(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task DeleteService(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task DeleteDeployment(YamlAppInfoDto dto, CancellationToken cancellationToken);
   
    public Task DeleteConfigMap(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task DeletePersistentVolume(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task DeletePersistentVolumeClaim(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task DeleteKeyVault(YamlAppInfoDto appInfoDto, CancellationToken cancellationToken);
    
    public Task DeleteIngress(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task DeleteDomainCertification(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task DeleteClusterRoleAndBindingAsync(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task DeleteNetaData(YamlAppInfoDto dto, CancellationToken cancellationToken);
}
