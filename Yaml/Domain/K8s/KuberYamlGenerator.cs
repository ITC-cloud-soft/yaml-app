using RazorLight;
using Yaml.Domain.K8s.Interface;
using Yaml.Infrastructure.CoustomService;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.YamlEum;

namespace Yaml.Domain.K8s;

public class KuberYamlGenerator : IKuberYamlGenerator 
{
    
    private readonly IRazorLightEngine _engine;
    private readonly ILogger _logger;
    private readonly string _currentDirectory = Directory.GetCurrentDirectory();
    private readonly IConfiguration _configuration;
    private readonly Utils _utils;
    public KuberYamlGenerator(IRazorLightEngine engine,
        ILogger<KuberYamlGenerator> logger,
        IConfiguration configuration,
        Utils utils)
    {
        _engine = engine;
        _logger = logger;
        _configuration = configuration;
        _utils = utils;
    }
 
    public async Task<string> GenerateConfigMap(YamlClusterInfoDto cluster)
    {
        if (!cluster.ConfigMapFlag)
        {
            return "";
        }
        var path = Path.Combine(_currentDirectory, KubeConstants.ConfigMapTemplate);
        return await _engine.CompileRenderAsync(path, cluster);
    }

    public async Task<string> GenerateConfigFile(YamlClusterInfoDto cluster)
    {
        if (cluster.ConfigMapFileFlag)
        {
            var path = Path.Combine(_currentDirectory, KubeConstants.ConfigFileTemplate);
            await _utils.UpdateClusterConfigFiel(cluster);
            return await _engine.CompileRenderAsync(path, cluster);
        }
        return "";
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

    public async Task<string> GeneratePersistentVolume(
        string pvName,
        string storage,
        string storageClassName,
        string SubscriptionId,
        string ResourceGroup)
    {
        var path = Path.Combine(_currentDirectory, KubeConstants.PersistentVolumeTemplate);
        var content = await _engine.CompileRenderAsync(path, new PvRender
        {
            Name = pvName,
            Storage = storage,
            StorageClassName = storageClassName,
            SubscriptionId = SubscriptionId,
            ResourceGroup = "saas-core" // ResourceGroup
                            
        });
        return content;
    }

    public async Task<string> GeneratePersistentVolumeList(YamlAppInfoDto appInfoDto, YamlClusterInfoDto cluster)
    {
        var pv = "";
        if (cluster.DiskInfoFlag)
        {
            for (var i = 0; i < cluster.DiskInfoList?.Count; i++)
            {
                // prepare the parameter
                var diskInfo = cluster.DiskInfoList[i];
                var pvName = $"{KubeConstants.PvSubPrefix}-{cluster.ClusterName?.ToLower()}-{i}";
                var storage = diskInfo.DiskSize;
                var storageClassName = diskInfo.DiskType;
                var subscriptionId = appInfoDto.KeyVault?.ManagedId;
                var resourceGroup = "saas-core";

                // generate pv content
                pv += await GeneratePersistentVolume(pvName, storage, storageClassName, subscriptionId, resourceGroup);
                pv += "\n---\n";
            }
            return pv;
        }
        return "";
    }

    public async Task<string> GeneratePersistentVolumeClaim(YamlClusterInfoDto cluster)
    {
        var pvc = "";
        if (cluster.DiskInfoFlag)
        {
            for (var i = 0; i < cluster.DiskInfoList?.Count; i++)
            {
                var diskInfoDto = cluster.DiskInfoList[i];
                diskInfoDto.PvName = $"{KubeConstants.PvSubPrefix}-{cluster.ClusterName?.ToLower()}-{i}";
                var path = Path.Combine(_currentDirectory, KubeConstants.PersistentVolumeClaimTemplate);
                pvc += await _engine.CompileRenderAsync(path, diskInfoDto);
                pvc += "\n---\n";
            }
            return pvc;
        }
        return "";
    }

    public async Task<string> GenerateSecret(YamlAppInfoDto appInfoDto, YamlClusterInfoDto cluster)
    {
        // key vaults belong to single cluster
        if (!appInfoDto.KeyVaultFlag)
        {
            return "";
        }
        
        var path = Path.Combine(_currentDirectory, KubeConstants.SecretTemplate);
   
        var keyVaultRender = new KeyVaultRender()
        {
            AppName = appInfoDto.AppName,
            KeyVaultName = appInfoDto.KeyVault?.KeyVaultName!,
            ManagedId = appInfoDto.KeyVault?.ManagedId!,
            TenantId = appInfoDto.KeyVault?.TenantId!,
            ConfigKeyList = cluster.KeyVault?.Select(kv => kv.ConfigKey!).ToList() ?? new List<string>(),
            IfAWS = appInfoDto.CloudType == CloudType.AWS
        };
    
        // key vaults belong to the whole application
        if (appInfoDto.KeyVault?.KeyVault != null)
        {
            keyVaultRender.ConfigKeyList.AddRange(
                appInfoDto.KeyVault.KeyVault
                    .Where(kv => kv.ConfigKey != null)  
                    .Select(kv => kv.ConfigKey!).ToList<string>()
            );
        }
        
        return await _engine.CompileRenderAsync(path, keyVaultRender);
    }

    public async Task<string> GenerateIngress(YamlClusterInfoDto cluster)
    {
        if (!cluster.domainFlag)
        {
            return "";
        }
        var path = Path.Combine(_currentDirectory, KubeConstants.IngressFileTemplate);
        return await _engine.CompileRenderAsync(path, cluster);
    }
    
    public async Task<string> GenerateIngressSecret(YamlAppInfoDto appInfoDto, YamlClusterInfoDto cluster)
    {
        if (!cluster.domainFlag)
        {
            return "";
        }
        
        if (cluster.Domain != null && String.IsNullOrEmpty(cluster.Domain.Certification) && String.IsNullOrEmpty(cluster.Domain.PrivateKey))
        {
            return "";
        }
        var uploadDirectory = _configuration.GetConnectionString("UploadDirectory"); // 修改这里的路径

        var certificationLocation = Path.Combine(uploadDirectory, cluster.Domain?.Certification ?? "");
        var privateKeyLocation = Path.Combine(uploadDirectory, cluster.Domain?.PrivateKey ?? "");

        // 读取证书和私钥的字节数据
        var certificationBytes = await File.ReadAllBytesAsync(certificationLocation);
        var privateKeyBytes = await File.ReadAllBytesAsync(privateKeyLocation);

        // 将字节数据转换为 Base64 字符串
        var certificationBase64 = Convert.ToBase64String(certificationBytes);
        var privateKeyBase64 = Convert.ToBase64String(privateKeyBytes);

        // 更新 cluster 对象的 Certification 和 PrivateKey 属性为 Base64 编码的字符串
        cluster.Domain!.Certification = certificationBase64;
        cluster.Domain!.PrivateKey = privateKeyBase64;
        
        var path = Path.Combine(_currentDirectory, KubeConstants.IngressSecretTemplate);
        return await _engine.CompileRenderAsync(path, cluster);
    }
}