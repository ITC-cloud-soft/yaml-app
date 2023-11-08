using Yaml.Domain.Entity;
using Yaml.Infrastructure.Mappings;

namespace Yaml.Infrastructure.Dto;
public class YamlAppInfoDto : IMapFrom<YamlAppInfo>
{
    public int Id{ get; set; }
    
    // Namespace '[a-z0-9]([-a-z0-9]*[a-z0-9])?'
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
    
    public AppKeyVault( string? tenantId, string? keyVaultName, string? managedId, List<string> keyVault)
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
    public List<string>? KeyVault { get; set; }
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
    
    // Disk Info 
    public bool DiskInfoFlag { set; get; }
    public  DiskInfo? Disk { set; get; }

    // KeyVault 
    public bool KeyVaultFlag { set; get; } 
    public List<KeyVaultDto>? KeyVault { set; get; }

    // ConfigMap
    public bool ConfigMapFlag { set; get; }
    public List<ConfigMapDto>? ConfigMap { set; get; }
    public bool ConfigMapFileFlag { set; get; }
    public List<ConfigFileDto>? ConfigFile { set; get; }
    
    // Domain
    public DomainDto? Domain { set; get; }
}

public class DiskInfo
{
    public string? Size { set; get; }
    public string? Type { set; get; }
    public MountPath[]? MountPath { set; get; }

}
public class MountPath
{
    public int Id { set; get; }
    public string? Name { set; get; }
    public string? Path { set; get; }
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
    public int?  Id { set; get; }
    public int?  AppId{ set; get; }
    public string? ConfigKey { set; get; }
    public string? Value { set; get; }
}