using System.Reflection;
using FluentValidation;
using RazorLight;
using RazorLight.Extensions;
using Yaml.Domain.AzureApi;
using Yaml.Domain.AzureApi.Interface;

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
        
        // 4.Azure Identity Manager
        services.AddSingleton<IAzureIdentityManager, AzureIdentityManager>(provider =>
        {
            string clientId = configuration["Azure:ClientId"] ?? "";
            string clientSecret = configuration["Azure:ClientSecret"] ?? "";
            string tenantId = configuration["Azure:TenantId"] ?? "";
            string subscriptionId = configuration["Azure:SubscriptionId"] ?? "";

            return new AzureIdentityManager(clientId, clientSecret, tenantId, subscriptionId);
        });
        
        // 5.Memory Cache
        services.AddMemoryCache();
        return services;
    }
}