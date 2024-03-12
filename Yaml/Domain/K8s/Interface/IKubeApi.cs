using k8s.Models;
using Microsoft.Azure.Management.Msi.Fluent;
using Yaml.Infrastructure.Dto;

namespace Yaml.Domain.K8s.Interface;
public interface IKubeApi
{
    public Task<V1Namespace> CreateNamespace(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task<V1Service[]> CreateService(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task<V1Deployment[]> CreateDeployment(YamlAppInfoDto dto, CancellationToken cancellationToken);
   
    public Task<V1ConfigMap?[]> CreateConfigMap(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task<List<V1PersistentVolume>> CreatePersistentVolume(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task<List<V1PersistentVolumeClaim>> CreatePersistentVolumeClaim(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task<string> CreateKeyVault(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    public Task<V1Ingress[]> CreateIngress(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task<V1Secret[]> CreateDomainCertification(YamlAppInfoDto dto, CancellationToken cancellationToken);
}
