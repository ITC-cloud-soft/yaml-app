
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yaml.Infrastructure.Presistence.dao;
namespace Yaml.Domain.Entity;

[Table("TBL_YAML_CLUSTER_CONFIG_MAP_FILE_INFO")]
public class YamlClusterConfigFileInfo : CommonFields
{
    [Key]
    [Column("id")]
    public int Id { set; get; }
    
    [Column("file_path")]
    public string? FilePath { set; get; }
    
    [Column("file_link")]
    public string? FileLink { set; get; }
    
    [Column("cluster_id")]
    public int? ClusterId{ set; get; }
}