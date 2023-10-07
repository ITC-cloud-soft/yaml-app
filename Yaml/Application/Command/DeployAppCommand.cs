using AutoMapper;
using MediatR;
using RazorLight;
using Yaml.Domain.K8s.Interface;
using Yaml.Infrastructure.Dto;

namespace Yaml.Application.Command;

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
        // var v1ConfigMaps = await _kubeApi.CreateConfigMap(command.AppInfoDto, cancellationToken);
        // await _kubeApi.CreatePersistentVolumeClaim(command.AppInfoDto, cancellationToken);
        await _kubeApi.CreateService(command.AppInfoDto, cancellationToken);
        return "1";
    }
}