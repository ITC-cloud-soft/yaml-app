using MediatR;
using Microsoft.AspNetCore.Mvc;
using Yaml.Application.Query;
using Yaml.Domain;
using Yaml.Domain.K8s.Interface;
using Yaml.Infrastructure.CoustomService;

namespace Yaml.Application.Command;

public class WithdrawClusterCommand: IRequest
{
    public int Id { get; set; }
}

public class WithdrawClusterCommandHandler : IRequestHandler<WithdrawClusterCommand>
{
    private readonly ILogger _logger;
    private readonly ISender _mediator;
    private readonly  IKubeApi _kubeApi;
    public WithdrawClusterCommandHandler(ILogger<WithdrawClusterCommandHandler> logger, ISender mediator, IKubeApi kubeApi)
    {
        _logger = logger;
        _mediator = mediator;
        _kubeApi = kubeApi;
    }

    public async Task Handle(WithdrawClusterCommand request, CancellationToken cancellationToken)
    {

        var appList = await _mediator.Send(new ListQuery(), cancellationToken);
        if (appList.Count == 0)
        {
            return;
        }
        var app = appList.First();
        if (app.K8sConfig)
        {
            try
            {
                await _kubeApi.DeleteDeployment(app, cancellationToken);
                await _kubeApi.DeleteService(app, cancellationToken);
                await _kubeApi.DeleteIngress(app, cancellationToken);
                await _kubeApi.DeletePersistentVolumeClaim(app, cancellationToken);
                await _kubeApi.DeleteDomainCertification(app, cancellationToken);
                await _kubeApi.DeleteConfigMap(app, cancellationToken);
                await _kubeApi.DeleteKeyVault(app, cancellationToken);
                await _kubeApi.DeleteNamespace(app, cancellationToken);
                _logger.LogInformation("withdraw Id with k8s configuration : {Id} succeed", request.Id);
            }
            catch (Exception exception)
            {
                _logger.LogError("Error when clearing application with k8s config: {}", exception.Message);
            }
        }
        else
        {
            try
            {
                // withdraw by command
                // delete apply kubectl delete all --all -n my-namespace
                var command = $" kubectl delete  all --all -n {app.AppName}-ns";
                var output = await ShellHelper.RunShellCommand(command);

                _logger.LogInformation(output);
                _logger.LogInformation("withdraw Id :{Id} succeed", request.Id);

                // delete namespace
                var createSpaceCommand = $"kubectl delete namespace {app.AppName}-ns";
                var res = await ShellHelper.RunShellCommand(createSpaceCommand);

                _logger.LogInformation(res);
                _logger.LogInformation("delete namespace :{Namespace} succeed", app.AppName + "-ns");
            }
            catch (Exception e)
            {
                _logger.LogError("Error when clearing application with k8s config: []", e.Message);

            }
        }
    }
}