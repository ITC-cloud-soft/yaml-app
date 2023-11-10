
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yaml.Infrastructure.Presistence.dao;
namespace Yaml.Domain.Entity;


[Table("TBL_YAML_CLUSTER_DOMAIN_INFO")]
public class YamlClusterDomainInfo : CommonFields
{
    [Column("id")]
    public int Id { set; get; }

    [Column("domain_name")]
    public string? DomainName { set; get; }
    
    
    [Column("certification")]
    public string? Certification { set; get; }
    
        
    [Column("private_key")]
    public string? PrivateKey { set; get; }
    
    [Column("cluster_id")]
    public int ClusterId{ set; get; }

    // public YamlClusterDomainInfo(int id)
    // {
    //     Id = id;
    // }
    //
    // public YamlClusterDomainInfo()
    // {
    //    
    // }
}