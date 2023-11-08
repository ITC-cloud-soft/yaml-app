using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Yaml.Application;
using Yaml.Infrastructure.Presistence.dao;
namespace Yaml.Domain.Entity;


[Table("TBL_YAML_CLUSTER_INFO")]
public class YamlClusterInfo : CommonFields
{
    [Required]
    [Key]
    public int Id { get; set; }
 
    [Column("cluster_name")]
    public string? ClusterName { get; set; }
    
    [Column("image")]
    public string? Image { get; set; }
    
    [Column("pod_num")]
    public int? PodNum { get; set; }
    
    [Column("cpu")]
    public string? Cpu { get; set; }
    
    [Column("memory")]
    public string? Memory { get; set; }
    
    [Column("managed_label")]
    public string? ManageLabel { get; set; }
    
    [Column("prefix")]
    public string? Prefix { get; set; }
    
    [Column("app_id")]
    public int? AppId { get; set; }
    
    [Column("keyVault_flag")]
    public bool KeyVaultFlag { get; set; }
    
    [Column("configmap_flag")]
    public bool ConfigMapFlag { get; set; }
    
    [Column("configmap_file_flag")]
    public bool ConfigMapFileFlag { get; set; }
}