using Microsoft.EntityFrameworkCore;
using Yaml.Domain.Entity;

public class MyDbContext : DbContext
{
    protected MyDbContext()
    {
        
    }

    public MyDbContext(DbContextOptions options) : base(options)
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
 	public DbSet<YamlKeyVaultInfo> KeyVaultInfo { get; set; }
 	public DbSet<YamlClusterDomainInfo> Domain { get; set; }
 	public DbSet<YamlClusterConfigFileInfo> ConfigFile { get; set; }
 	public DbSet<YamlClusterConfigMapInfo> ConfigMap { get; set; }
}