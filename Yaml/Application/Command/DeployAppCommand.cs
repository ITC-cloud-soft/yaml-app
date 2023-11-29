using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using RazorLight;
using Yaml.Domain.AzureApi.Interface;
using Yaml.Domain.K8s.Interface;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.Exception;

namespace Yaml.Application.Command;

public class DeployAppCommand : IRequest<string>
{
    [Required]
    public YamlAppInfoDto? AppInfoDto { get; set; }
}

public class DeployAppCommandHandler : IRequestHandler<DeployAppCommand, string>
{
    private readonly ILogger _logger;
    private readonly  IRazorLightEngine _engine;
    private readonly  IKubeApi _kubeApi;

    public DeployAppCommandHandler(
        MyDbContext context, 
        IMapper mapper, 
        ILogger<DeployAppCommandHandler> logger,
        IRazorLightEngine razorLightEngine,
        IKubeApi kubeApi
        )
    {
        _logger = logger;
        _engine = razorLightEngine;
        _kubeApi = kubeApi;
    }

    public async Task<string> Handle(DeployAppCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var v1Namespace = await _kubeApi.CreateNamespace(command.AppInfoDto, cancellationToken);
            // await _kubeApi.CreateAzureIdentityAsync(command.AppInfoDto, cancellationToken);
            // await _kubeApi.CreateKeyVault(command.AppInfoDto, cancellationToken);
            // await _kubeApi.CreateDomainCertification(command.AppInfoDto, cancellationToken);
            await _kubeApi.CreateIngress(command.AppInfoDto, cancellationToken);
            // await _kubeApi.CreateConfigMap(command.AppInfoDto, cancellationToken);
            // await _kubeApi.CreatePersistentVolumeClaim(command.AppInfoDto, cancellationToken);
            // await _kubeApi.CreateService(command.AppInfoDto, cancellationToken);
            // await _kubeApi.CreateDeployment(command.AppInfoDto, cancellationToken);
            return v1Namespace.Metadata.Name;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new ServiceException("Deploy APP Error : ", e);
        }
    }
}