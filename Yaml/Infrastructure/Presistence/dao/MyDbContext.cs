using Microsoft.EntityFrameworkCore;
using Yaml.Domain.Entity;
using Yaml.Infrastructure.Dto;

public class MyDbContext : DbContext
{
    protected MyDbContext()
    {
        
    }

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Key configuration
        modelBuilder.Entity<YamlAppInfo>()
            .Property(e => e.Id);
    }

 	public DbSet<YamlAppInfo> AppInfoContext { get; set; }
 	public DbSet<YamlClusterInfo> ClusterContext { get; set; }
    
 	public DbSet<YamlClusterDiskInfo> DiskInfoContext{ get; set; }
 	public DbSet<YamlKeyVaultInfo> KeyVaultInfoContext { get; set; }
 	public DbSet<YamlClusterDomainInfo> DomainContext { get; set; }
 	public DbSet<YamlClusterConfigFileInfo> ConfigFile { get; set; }
 	public DbSet<YamlClusterConfigMapInfo> ConfigMap { get; set; }
 	public DbSet<YamlUser> User { get; set; }
}