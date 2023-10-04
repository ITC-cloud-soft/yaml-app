using k8s.Models;
using Yaml.Infrastructure.Dto;

namespace Yaml.Infrastructure.K8s;

public interface IKubeApi
{
    /// <summary>
    /// create a new namespace
    /// </summary>
    /// <param name="namespaceName">name of namespace</param>
    /// <param name="cancellationToken">cancellation token。</param>
    /// <returns>Info of the namespace</returns>
    public Task<V1Namespace> CreateNamespace(string namespaceName, CancellationToken cancellationToken);

    /// <summary>
    /// create service in k8s
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public Task<string> CreateService(YamlAppInfoDto dto, CancellationToken cancellationToken);
}