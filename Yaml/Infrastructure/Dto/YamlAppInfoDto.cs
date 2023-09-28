using Yaml.Domain.Entity;

namespace Yaml.Infrastructure.Dto;
public class YamlAppInfoDto : IMapFrom<YamlAppInfo>
{
    public int? Id;
    public string? AppName { get; set; }
    public string? Cr { get; set; }
    public string? Token { get; set; }
    public string? MailAddress { get; set; }
    
    public bool NetdataFlag { get; set; }
    
    public bool KeyVaultFlag { get; set; }
    public AppKeyVault? KeyVault { get; set; }
    
    public List<YamlClusterInfoDto> ClusterInfoList { get; set; }
}

public class AppKeyVault
{
    public string? TenantId { get; set; }
    public string? KeyVaultName { get; set; }
    public string? ManagedId { get; set; }
    public List<string>? KeyVault { get; set; }
}


public class YamlClusterInfoDto : IMapFrom<YamlAppInfo>
{
    public string? ClusterName { set; get; }
    public string? Image { set; get; }
    public int? PodNum { set; get; }
    public string? Cpu { set; get; }
    public string? Memory { set; get; }
    public string? ManageLabel { set; get; }
    public string? Prefix { set; get; }
    
    // Disk Info 
    public bool DiskInfoFlag { set; get; }
    public DiskInfo? Disk;

    // KeyVault 
    public bool KeyVaultFlag { set; get; } 
    public string[] KeyVault { set; get; }

    // ConfigMap
    public bool ConfigMapFlag { set; get; }
    public List<ConfigMapDto> ConfigMap { set; get; }
    
    public bool ConfigMapFileFlag { set; get; }
    public List<ConfigFileDto> ConfigFile { set; get; }
    
    // Domain
    public List<DomainDto> DomainList { set; get; }

}

public class DiskInfo
{
    public int? Size { set; get; }
    public string? Type { set; get; }
    public string[]? MountPath { set; get; }

}
public class ConfigMapDto
{
    public string? ConfigKey { set; get; }
    public string? Value { set; get; }
}

public class ConfigFileDto
{
    public string? FilePath { set; get; }
    public string? FileLink { set; get; }
}

public class DomainDto
{
    public string? DomainName { set; get; }
    public string? Certification { set; get; }
    public string? PrivateKey { set; get; }
}

public class KeyVaultDto
{
    public string? Key { set; get; }
    public string? Value { set; get; }
}