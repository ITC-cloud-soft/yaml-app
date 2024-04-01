using System.ComponentModel.DataAnnotations.Schema;

namespace Yaml.Infrastructure.Presistence.dao;

public class CommonFields
{
    
    [Column("create_date")]
    public DateTime? CreateDate { get; set; }
    
    [Column("create_user")]
    public string? CreateUser { get; set; }
    
    [Column("update_date")]
    public DateTime? UpdateDate { get; set; }
    
    [Column("update_user")]
    public string? UpdateUser { get; set; }
}