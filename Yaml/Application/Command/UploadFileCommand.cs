using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Yaml.Application.Command;

public class UploadFileCommand : IRequest<IEnumerable<string>>
{
    public IFormFileCollection Files { get; set; }
}

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, IEnumerable<string>>
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;


    public UploadFileCommandHandler(ILogger<UploadFileCommandHandler> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IEnumerable<string>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var uploadDirectory = _configuration.GetConnectionString("UploadDirectory"); // 修改这里的路径

        try
        {
            var files = request.Files;
            if (files.Count == 0)
            {
                return Enumerable.Empty<string>();
            }

            if (uploadDirectory != null && !Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            var uploadedFiles = new List<string>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    Guid newGuid = Guid.NewGuid();
                    string prefix = newGuid.ToString();
                    var fileName = prefix + "_" + file.FileName;
                    var filePath = Path.Combine(uploadDirectory, fileName);
                    await using var stream = new FileStream(filePath, FileMode.Create) ;
                    await file.CopyToAsync(stream, cancellationToken);
                    uploadedFiles.Add(fileName);
                }
            }
            return uploadedFiles;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }
}