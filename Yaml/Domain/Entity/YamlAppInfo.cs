using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yaml.Infrastructure.Presistence.dao;
using Yaml.Infrastructure.YamlEum;

namespace Yaml.Domain.Entity;


[Table("TBL_YAML_APP_INFO")]
public class YamlAppInfo : CommonFields
{
    [Required] 
    [Key]
    public int Id { get; set; } 

    [Column("app_name")] 
    public string? AppName { get; set; } 
    
    [Column("cr")] 
    public string? Cr { get; set; }
    
    [Column("token")] 
    public string? Token { get; set; }
    
    [Column("mail_address")]
    public string? MailAddress { get; set; }
    
    [Column("cloud_type")]
    public CloudType? CloudType { get; set; }

    [Column("keyvault_flag")]
    public bool KeyVaultFlag { get; set; }
    
    [Column("tenantid")]
    public string? Tenantid { get; set; }
    
    [Column("key_vault_name")]
    public string? KeyVaultName { get; set; }

    [Column("managed_id")]
    public string? ManagedId { get; set; }

    [Column("netdata_flag")] 
    public bool NetdataFlag { get; set; } = false;
    
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("Kube_Config")]
    public string? KubeConfig { get; set; }
    
    [Column("K8s_Config")]
    public bool? K8sConfig { get; set; }
}