using System.Text;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Yaml.Application.Query;
using Yaml.Domain.K8s.Interface;
using Yaml.Infrastructure.Dto;

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
            var tasks =  yamlAppInfoDto.ClusterInfoList?.Select(cluster =>
            {
                var generateConfigMap =    _kuberYamlGenerator.GenerateConfigMap(cluster);
                return generateConfigMap;
            });
            var whenAll = await Task.WhenAll(tasks);
            var res = String.Join("---\n", whenAll);

            var fileBytes = Encoding.UTF8.GetBytes(res);
            var file = new FileContentResult(fileBytes, "application/json") { FileDownloadName = $"{yamlAppInfoDto.AppName}.yaml" };
            return file;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}