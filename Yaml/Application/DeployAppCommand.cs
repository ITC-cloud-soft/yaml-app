using AutoMapper;
using k8s;
using k8s.Models;
using MediatR;
using RazorLight;
using Yaml.Infrastructure.Dto;
namespace Yaml.Application;

public class DeployAppCommand : IRequest<string>
{
    public YamlAppInfoDto AppInfoDto { get; set; }
}

public class DeployAppCommandHandler : IRequestHandler<DeployAppCommand, string>
{
    private readonly MyDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;
    private const string TemplateFilePath = "YamlFile/YamlTemplate.cshtml";
    private const string OutPutFile = "YamlFile/App.yaml";
    private readonly  IRazorLightEngine _engine;


    public DeployAppCommandHandler(
        MyDbContext context, 
        IMapper mapper, 
        ILogger<DeployAppCommandHandler> logger,
        IRazorLightEngine razorLightEngine)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _engine = razorLightEngine;
    }

    public async Task<string> Handle(DeployAppCommand command, CancellationToken cancellationToken)
    {   
        // 1.create yaml file
        var currentDirectory = Directory.GetCurrentDirectory();
        var path = Path.Combine(currentDirectory, TemplateFilePath);
        
        var content = await _engine.CompileRenderAsync(path, command.AppInfoDto);
        await File.WriteAllTextAsync(Path.Combine(currentDirectory, OutPutFile), content, cancellationToken);
        
        // 2.manipulate k8s
        var config = new KubernetesClientConfiguration { Host = "http://127.0.0.1:8001", AccessToken = ""};
        var client = new Kubernetes(config);
        
        var pods = await client.ListPodForAllNamespacesAsync(cancellationToken: cancellationToken);
        foreach (var pod in pods.Items)
        {
            Console.WriteLine($"Namespace: {pod.Metadata.NamespaceProperty}, Pod Name: {pod.Metadata.Name}");
        }

        await client.CreateNamespaceAsync(new V1Namespace
        {
            Metadata = new V1ObjectMeta
            {
                Name = "gitlab" // 指定要创建的命名空间的名称
            }
        }, cancellationToken: cancellationToken);
        var v1Pod = KubernetesYaml.Deserialize<V1PersistentVolumeClaim>(content);
        var claim = await client.CreateNamespacedPersistentVolumeClaimAsync(v1Pod, "gitlab", cancellationToken: cancellationToken);
        return "1";
    }
}