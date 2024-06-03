using Ardalis.GuardClauses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Yaml.Domain;

namespace Yaml.Application.Query;

public class DownloadManualCommand: IRequest<FileContentResult>
{
    public string FileName { get; set; }
}

public class DownloadManualCommandHandler : IRequestHandler<DownloadManualCommand, FileContentResult>
{
    private readonly string _currentDirectory = Directory.GetCurrentDirectory();
    private readonly ILogger _logger;

    public DownloadManualCommandHandler(ILogger<DownloadManualCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<FileContentResult> Handle(DownloadManualCommand request, CancellationToken cancellationToken)
    {
        Guard.Against.NotFound("FileName", request.FileName);
        _logger.LogInformation("Downloading manual : {FileName}",  request.FileName);
        
        var bytes = await File.ReadAllBytesAsync(Path.Combine(KubeConstants.ManualFilePath, request.FileName), cancellationToken);
        return new FileContentResult(bytes, "application/json") { FileDownloadName = request.FileName };
    }
}