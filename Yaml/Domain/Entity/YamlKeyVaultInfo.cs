using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yaml.Infrastructure.Presistence.dao;
namespace Yaml.Domain.Entity;

[Table("TBL_YAML_KEY_VAULT_INFO")]
public class YamlKeyVaultInfo : CommonFields
{
    [Key]
    [Column("id")]
    public int  Id { set; get; }
    
    [Column("config_key")]
    public string? ConfigKey{ set; get; }
    
    [Column("value")]
    public string? Value{ set; get; }
    
    [Column("cluster_id")]
    public int?  ClusterId{ set; get; }
    
    [Column("app_id")]
    public int?  AppId{ set; get; }
}