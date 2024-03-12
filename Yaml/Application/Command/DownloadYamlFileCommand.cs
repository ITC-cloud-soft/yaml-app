using System.Text;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Yaml.Application.Query;
using Yaml.Domain;
using Yaml.Domain.K8s.Interface;

namespace Yaml.Application.Command;

public class DownloadYamlFileCommand : IRequest<FileContentResult>
{
    public int appId { get; set; }
}

public class DownloadYamlFileCommandHandler : IRequestHandler<DownloadYamlFileCommand, FileContentResult>
{
    private readonly ILogger _logger;
    private readonly ISender _mediator;
    private readonly IKuberYamlGenerator _kuberYamlGenerator;
    private const string NextLine = "\n---\n";
    private readonly string _currentDirectory = Directory.GetCurrentDirectory();
    public DownloadYamlFileCommandHandler(
        ILogger<DownloadYamlFileCommandHandler> logger,
        ISender mediator,
        IKuberYamlGenerator kuberYamlGenerator)
    {
        _logger = logger;
        _mediator = mediator;
        _kuberYamlGenerator = kuberYamlGenerator;
    }

    public async Task<FileContentResult> Handle(DownloadYamlFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var yamlAppInfoDto = await _mediator.Send(new GetAppQuery { AppId = request.appId });
            
            var tasks = yamlAppInfoDto.ClusterInfoList?.Select(async cluster =>
            {
                cluster.AppName = yamlAppInfoDto.AppName;
                var service = await _kuberYamlGenerator.GenerateService(cluster);
                var deployment = await _kuberYamlGenerator.GenerateDeployment(cluster);
                var configMap = await _kuberYamlGenerator.GenerateConfigMap(cluster);
                var ingress = await _kuberYamlGenerator.GenerateIngress(cluster);
                var persistentVolumeClaim = await _kuberYamlGenerator.GeneratePersistentVolumeClaim(cluster);
                var secret = await _kuberYamlGenerator.GenerateSecret(yamlAppInfoDto, cluster);

                return $"{service}" +
                       $"{NextLine}" +
                       $"{secret}" +
                       $"{NextLine}" +
                       $"{configMap}" +
                       $"{NextLine}" +
                       $"{ingress}" +
                       $"{NextLine}" +
                       $"{persistentVolumeClaim}" +
                       $"{deployment}";
            });
            
            var whenAll = await Task.WhenAll(tasks);
            var fileContent = String.Join(NextLine, whenAll);
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            
            await File.WriteAllTextAsync(
                Path.Combine(_currentDirectory, KubeConstants.OutPutFile),
                fileContent,
                cancellationToken);
            
            return new FileContentResult(fileBytes, "application/json") { FileDownloadName = $"{yamlAppInfoDto.AppName}.yaml" };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _logger.LogError("Error generate yaml file error", e);
            throw;
        }
    }
}