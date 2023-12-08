using Yaml.Domain.Entity;
using Yaml.Infrastructure.Mappings;

namespace Yaml.Infrastructure.Dto;

public class YamlAppInfoDto : IMapFrom<YamlAppInfo>
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public string? AppName { get; set; }
    public string? Cr { get; set; }
    public string? Token { get; set; }
    public string? MailAddress { get; set; }

    public bool NetdataFlag { get; set; }
    public bool KeyVaultFlag { get; set; }
    public AppKeyVault? KeyVault { get; set; }

    public List<YamlClusterInfoDto>? ClusterInfoList { get; set; }
}

public class AppKeyVault
{
    public AppKeyVault()
    {
    }

    public AppKeyVault(string? tenantId, string? keyVaultName, string? managedId, List<KeyVaultDto> keyVault)
    {
        TenantId = tenantId;
        KeyVaultName = keyVaultName;
        ManagedId = managedId;
        KeyVault = keyVault;
    }

    public int Id { get; set; }
    public string? TenantId { get; set; }
    public string? KeyVaultName { get; set; }
    public string? ManagedId { get; set; }
    public List<KeyVaultDto>? KeyVault { get; set; }
}

public class YamlClusterInfoDto : IMapFrom<YamlClusterInfo>
{
    public string? AppName { set; get; }
    public int? Id { set; get; }
    public string? ClusterName { set; get; }
    public string? Image { set; get; }
    public int? PodNum { set; get; }
    public string? Cpu { set; get; }
    public string? Memory { set; get; }
    public string? ManageLabel { set; get; }
    public string? Prefix { set; get; }

    // KeyVault 
    public bool KeyVaultFlag { set; get; }
    public List<KeyVaultDto>? KeyVault { set; get; }

    // ConfigMap
    public bool ConfigMapFlag { set; get; }
    public List<ConfigMapDto>? ConfigMap { set; get; }

    // ConfigFile
    public bool ConfigMapFileFlag { set; get; }
    public List<ConfigFileDto>? ConfigFile { set; get; }

    // DomainDto
    public DomainDto? Domain { set; get; }
    
    // Disk Info 
    public bool DiskInfoFlag { set; get; }
    public List<DiskInfoDto>? DiskInfoList { set; get; }
}

public class DiskInfoDto
{
    public int Id { set; get; }
    public int ClusterId { set; get; }

    public string? AppName { set; get; }
    public string? ClusterName { set; get; }
    public string? Name { set; get; }
    public string? Path { set; get; }
    
    public string? DiskSize { get; set; }
    
    public string? PvcName { get; set; }
    
    public string? DiskType { get; set; }
    
}

public class ConfigMapDto
{
    public int Id { set; get; }
    public string? ConfigKey { set; get; }
    public string? Value { set; get; }
}

public class ConfigFileDto
{
    public int Id { set; get; }
    public string? FilePath { set; get; }
    public string? FileLink { set; get; }
    public string? FileContent { set; get; }
}

public class DomainDto
{
    public int Id { set; get; }
    public string? DomainName { set; get; }
    public string? Certification { set; get; }
    public string? PrivateKey { set; get; }
}

public class KeyVaultDto
{
    public int Id { set; get; }
    public int? AppId { set; get; }
    public string? ConfigKey { set; get; }
    public string? Value { set; get; }
    public int? ClusterId { set; get; }
}