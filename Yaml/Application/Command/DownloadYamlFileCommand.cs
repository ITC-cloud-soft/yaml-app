using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Yaml.Application.Command;

public class DownloadYamlFileCommand : IRequest<FileContentResult>
{
    public int appId { get; set; }
}

public class DownloadYamlFileCommandHandler : IRequestHandler<DownloadYamlFileCommand, FileContentResult>
{
    public Task<FileContentResult> Handle(DownloadYamlFileCommand request, CancellationToken cancellationToken)
    {
        


        return null;
    }
}