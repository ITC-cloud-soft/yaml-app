using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yaml.Infrastructure.Presistence.dao;

namespace Yaml.Domain.Entity;

[Table("TBL_YAML_User")]
public class YamlUser : CommonFields
{
    [Key]
    [Required] 
    public int Id { get; set; } 
    
    [Column("NAME")]
    public string? Name { get; set; } 
   
    [Column("PASSWORD")]
    public string? Password { get; set; }
    
    [Column("TOKEN")]
    public string? Token { get; set; } 
    
    [Column("MAIL_ADDRESS")]
    public string? MailAddress { get; set; } 

}