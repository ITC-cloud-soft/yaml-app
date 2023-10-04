using AutoMapper;
using k8s;
using k8s.Models;
using MediatR;
using RazorLight;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.K8s;

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
    private const string TemplateFilePath = "YamlFile/Service.cshtml";
    private const string OutPutFile = "YamlFile/App.yaml";
    private readonly  IRazorLightEngine _engine;
    private readonly  IKubeApi _kubeApi;


    public DeployAppCommandHandler(
        MyDbContext context, 
        IMapper mapper, 
        ILogger<DeployAppCommandHandler> logger,
        IRazorLightEngine razorLightEngine,
        IKubeApi kubeApi)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _engine = razorLightEngine;
        _kubeApi = kubeApi;
    }

    public async Task<string> Handle(DeployAppCommand command, CancellationToken cancellationToken)
    {   
        // 1.create yaml file
        var currentDirectory = Directory.GetCurrentDirectory();
        var path = Path.Combine(currentDirectory, TemplateFilePath);
        
        var content = await _engine.CompileRenderAsync(path, command.AppInfoDto);
        await File.WriteAllTextAsync(Path.Combine(currentDirectory, OutPutFile), content, cancellationToken);
        
        // 2.manipulate k8s
        return await _kubeApi.CreateService(command.AppInfoDto, cancellationToken);
    }
}