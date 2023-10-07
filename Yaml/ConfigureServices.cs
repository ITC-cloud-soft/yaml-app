

using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;
using RazorLight.Extensions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
        
        // Inject RazorLight into Container
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
        
        // Swagger
        services.AddSwaggerGen(options =>
        {
            options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        });
        return services;
    }
}