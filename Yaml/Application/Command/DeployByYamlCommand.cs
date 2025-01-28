using System.ComponentModel.DataAnnotations;
using MediatR;
using Yaml.Application.Query;
using Yaml.Domain;
using Yaml.Infrastructure.CoustomService;

namespace Yaml.Application.Command;

public class DeployByYamlCommand:IRequest
{
    [Required]
    public int AppId { get; set; }
}

public class DeployByYamlCommandHandler : IRequestHandler<DeployByYamlCommand>
{
    private readonly ILogger _logger;
    private readonly ISender _mediator;
    private readonly string _currentDirectory = Directory.GetCurrentDirectory();
    public DeployByYamlCommandHandler(ILogger<DeployByYamlCommandHandler> logger, ISender mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task Handle(DeployByYamlCommand request, CancellationToken cancellationToken)
    {
        DownloadYamlFileCommand downloadCommand = new DownloadYamlFileCommand { appId = request.AppId };
        await _mediator.Send(downloadCommand, cancellationToken);
        var filePath = Path.Combine(_currentDirectory, KubeConstants.OutPutFile); 
        
        
        var app = await _mediator.Send(new GetAppQuery { AppId = request.AppId, UserId = 1 }, cancellationToken);
        try
        {
            var createSpaceCommand = $"kubectl create namespace {app.AppName}-ns";
            var res = await ShellHelper.RunShellCommand(createSpaceCommand);

            _logger.LogInformation(res);
            _logger.LogInformation("create namespace :{Namespace} succeed", app.AppName +"-ns");
            
            var command = $"kubectl apply -f {filePath} --namespace {app.AppName}-ns";
            var output = await ShellHelper.RunShellCommand(command);

            _logger.LogInformation(output);
            _logger.LogInformation("deploy Id :{Id} succeed", downloadCommand.appId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception occurred {ex} ", ex);
            throw ex;
        }
    }
}