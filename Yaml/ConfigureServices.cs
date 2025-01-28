using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using RazorLight;
using RazorLight.Extensions;

namespace Yaml;

public static class ConfigureServices
{
    public static IServiceCollection AppServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
        });
        // 1.CROS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin", builder =>
            {
                builder
                    .WithOrigins("*") 
                    .AllowAnyMethod() 
                    .AllowAnyHeader(); 
            });
        });
        
        // 2.Inject RazorLight into Container
        services.AddRazorLight(() =>
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var engine = new RazorLightEngineBuilder()
                .SetOperatingAssembly(typeof(Program).Assembly)
                .UseEmbeddedResourcesProject(typeof(Program))
                .UseFileSystemProject(currentDirectory)
                .UseMemoryCachingProvider()
                .DisableEncoding()
                .Build();
            return engine;
        });
        
        // 3.Swagger
        services.AddSwaggerGen(options =>
        {
            options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        });
        
        // 5.Memory Cache
        services.AddMemoryCache();
        Console.WriteLine(configuration.GetConnectionString("MyDatabase"));
        services.AddDbContext<MyDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("MyDatabase")));

        return services;
    }
}