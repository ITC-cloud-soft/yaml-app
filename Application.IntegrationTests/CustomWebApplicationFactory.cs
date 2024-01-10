
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace YamlTest;

public class CustomWebApplicationFactory : WebApplicationFactory<Testing>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            var integrationConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            configurationBuilder.AddConfiguration(integrationConfig);
        });

        builder.ConfigureServices((builder, services) =>
        {
            services
                .Remove<DbContextOptions<MyDbContext>>()
                .AddDbContext<MyDbContext>((sp, options) =>
                    options.UseMySql(
                        builder.Configuration.GetConnectionString("DefaultConnection"),
                        new MySqlServerVersion(new Version(8, 0, 21)), // 替换为您的MySQL服务器版本
                        builder => builder.MigrationsAssembly(typeof(MyDbContext).Assembly.FullName))
                    );
        });
    }
}
