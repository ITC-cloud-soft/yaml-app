using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yaml.Infrastructure.Presistence.dao;
namespace Yaml.Domain.Entity;

[Table("MT_KBN")]
public class ClusterConfig : CommonFields
{
    [Required]
    [Column("KBNCODE")]
    public string? Code { get; set; }
    
    [Required]
    [Column("KBNVALUE")]
    public string? Value { get; set; } 
    
    [Required]
    [Column("KBNName")]
    public string? Name { get; set; }
    
    [Required]
    [Column("KBNOrder")]
    public int Order { get; set; }
    
    [Required]
    [Column("COMMENT")]
    public string? Comment { get; set; }

}