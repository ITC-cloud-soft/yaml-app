using System.ComponentModel.DataAnnotations;
using MediatR;
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
    private readonly  IKubeApi _kubeApi;

    public DeployAppCommandHandler(
        ILogger<DeployAppCommandHandler> logger,
        IKubeApi kubeApi)
    {
        _logger = logger;
        _kubeApi = kubeApi;
    }

    public async Task<string> Handle(DeployAppCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var v1Namespace = await _kubeApi.CreateNamespace(command.AppInfoDto, cancellationToken);
            await _kubeApi.CreateKeyVault(command.AppInfoDto, cancellationToken);
            await _kubeApi.CreateConfigMap(command.AppInfoDto, cancellationToken);
            await _kubeApi.CreateConfigMapFile(command.AppInfoDto, cancellationToken);
            await _kubeApi.CreateDomainCertification(command.AppInfoDto, cancellationToken);
            await _kubeApi.CreatePersistentVolumeClaim(command.AppInfoDto, cancellationToken);
            await _kubeApi.CreateIngress(command.AppInfoDto, cancellationToken);
            await _kubeApi.CreateService(command.AppInfoDto, cancellationToken);
            await _kubeApi.CreateDeployment(command.AppInfoDto, cancellationToken);

            if (command.AppInfoDto.NetdataFlag)
            {
                await _kubeApi.CreateClusterRoleAndBindingAsync(command.AppInfoDto, cancellationToken);
                await _kubeApi.DeployNetaData(command.AppInfoDto, cancellationToken);
            }
            return v1Namespace.Metadata.Name;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Deploy APP Error: {[]}", e.Message);
            throw new ServiceException("Deploy APP Error : ", e);
        }
    }
}