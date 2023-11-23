using k8s.Models;
using Yaml.Infrastructure.Dto;

namespace Yaml.Domain.K8s.Interface;
public interface IKubeApi
{
    /// <summary>
    /// 指定した名前で新しいネームスペースを作成します。
    /// </summary>
    /// <param name="namespaceName">ネームスペースの名前</param>
    /// <param name="cancellationToken">操作をキャンセルするためのトークン</param>
    /// <returns>作成されたネームスペースの情報</returns>
    public Task<V1Namespace> CreateNamespace(YamlAppInfoDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// YamlAppInfoDtoを使用して新しいサービスを作成します。
    /// </summary>
    /// <param name="dto">YamlAppInfoDtoの情報</param>
    /// <param name="cancellationToken">操作をキャンセルするためのトークン</param>
    /// <returns>作成されたサービスの情報</returns>
    public Task<V1Service[]> CreateService(YamlAppInfoDto dto, CancellationToken cancellationToken);
   
    /// <summary>
    /// YamlAppInfoDtoを使用して新しいデプロイメントを作成します。
    /// </summary>
    /// <param name="dto">YamlAppInfoDtoの情報</param>
    /// <param name="cancellationToken">操作をキャンセルするためのトークン</param>
    /// <returns>作成されたデプロイメントの情報</returns>
    public Task<V1Deployment[]> CreateDeployment(YamlAppInfoDto dto, CancellationToken cancellationToken);
   
    /// <summary>
    /// YamlAppInfoDtoを使用して新しいConfigMapを作成します。
    /// </summary>
    /// <param name="dto">YamlAppInfoDtoの情報</param>
    /// <param name="cancellationToken">操作をキャンセルするためのトークン</param>
    /// <returns>作成されたConfigMapの情報の配列</returns>
    public Task<V1ConfigMap?[]> CreateConfigMap(YamlAppInfoDto dto, CancellationToken cancellationToken);
   
    /// <summary>
    /// YamlAppInfoDtoを使用して新しい永続ボリュームを作成します。
    /// </summary>
    /// <param name="dto">YamlAppInfoDtoの情報</param>
    /// <param name="cancellationToken">操作をキャンセルするためのトークン</param>
    /// <returns>作成された永続ボリュームの情報</returns>
    public Task<V1PersistentVolume> CreatePersistentVolume(YamlAppInfoDto dto, CancellationToken cancellationToken);
    
    /// <summary>
    /// YamlAppInfoDtoを使用して新しい永続ボリュームクレームを作成します。
    /// </summary>
    /// <param name="dto">YamlAppInfoDtoの情報</param>
    /// <param name="cancellationToken">操作をキャンセルするためのトークン</param>
    /// <returns>作成された永続ボリュームクレームの情報の配列</returns>
    public Task<V1PersistentVolumeClaim[]> CreatePersistentVolumeClaim(YamlAppInfoDto dto, CancellationToken cancellationToken);

    public Task<V1Secret> CreateSecret(YamlAppInfoDto dto, CancellationToken cancellationToken);
    public Task<V1Ingress[]> CreateIngress(YamlAppInfoDto dto, CancellationToken cancellationToken);
}
