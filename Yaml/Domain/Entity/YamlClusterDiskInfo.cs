using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yaml.Infrastructure.Presistence.dao;

namespace Yaml.Domain.Entity;
[Table("TBL_YAML_CLUSTER_DISK_INFO")]
public class YamlClusterDiskInfo : CommonFields
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("name")]
    public string? Name { get; set; }
    
    [Column("path")]
    public string? Path { get; set; }
    
    [Column("cluster_id")]
    public int ClusterId { get; set; }
}