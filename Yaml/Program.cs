using k8s;
using Microsoft.EntityFrameworkCore;
using Yaml;
using Yaml.Infrastructure.Exception;
using Yaml.Infrastructure.K8s;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AppServiceConfiguration(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IKubeApi, KubeCli>();
// 注册Kubernetes客户端（如果需要）
builder.Services.AddSingleton<Kubernetes>(_ =>
{
    var config = new KubernetesClientConfiguration
    {
        Host = "http://127.0.0.1:8001",
    };
    return new Kubernetes(config);
});

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 32)))); // Replace with your MySQL version
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseMiddleware<ExceptionHandlingInterceptor>();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
