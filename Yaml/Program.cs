using System.Globalization;
using Azure.Storage.Blobs;
using k8s;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Yaml.Domain.K8s;
using Yaml.Domain.K8s.Interface;
using Yaml.Infrastructure.CoustomService;
using Yaml.Infrastructure.Exception;
namespace Yaml;

public class Program
{
    public static void Main(string[] args)
    {
        
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AppServiceConfiguration(builder.Configuration);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<Kubernetes>(_ =>
        {
            var config = new KubernetesClientConfiguration
            {
                Host = "http://127.0.0.1:8001",
            };
            return new Kubernetes(config);
        });
        
        // localization service
        builder.Services.AddLocalization(options => options.ResourcesPath = "Localization");
        builder.Services.Configure<RequestLocalizationOptions>(
            opts =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en-us"),
                    new CultureInfo("en"),
                    new CultureInfo("zh"),
                    new CultureInfo("ja"),
                    new CultureInfo("en-jp"),
                };

                opts.DefaultRequestCulture = new RequestCulture("ja");
                // Formatting numbers, dates, etc.
                opts.SupportedCultures = supportedCultures;
                // UI strings that we have localized.
                opts.SupportedUICultures = supportedCultures;
            });
                
        builder.Services.AddControllersWithViews()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();
        
        builder.Services.AddScoped<IKubeApi, KubeApi>();
        builder.Services.AddScoped<IKuberYamlGenerator, KuberYamlGenerator>();

        var app = builder.Build();
        app.UseRequestLocalization();
        var rewriteOptions = new RewriteOptions()
            .AddRewrite("^$", "cd-control.html", skipRemainingRules: true);
        app.UseRewriter(rewriteOptions);
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }
        app.UseRouting();
        app.UseCors("AllowSpecificOrigin"); // use CORS named "AllowSpecificOrigin"
        app.UseMiddleware<ExceptionHandlingInterceptor>();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.MapControllers();
        
        
        // 确保在应用启动时应用任何挂起的迁移
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            dbContext.Database.Migrate();
        }
        app.Run();
    }
}