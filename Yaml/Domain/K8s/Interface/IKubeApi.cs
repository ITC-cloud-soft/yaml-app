using k8s.Models;
using Microsoft.Azure.Management.Msi.Fluent;
using Yaml.Infrastructure.Dto;

namespace Yaml.Domain.K8s.Interface;
public interface IKubeApi
{
    public Task<V1Namespace> CreateNamespace(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task CreateService(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task CreateDeployment(YamlAppInfoDto dto, CancellationToken cancellationToken);
   
    public Task CreateConfigMap(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task<List<V1PersistentVolume>> CreatePersistentVolume(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task CreatePersistentVolumeClaim(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task CreateKeyVault(YamlAppInfoDto appInfoDto, CancellationToken cancellationToken);
    
    public Task CreateIngress(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task<V1Secret[]> CreateDomainCertification(YamlAppInfoDto dto, CancellationToken cancellationToken);
}
