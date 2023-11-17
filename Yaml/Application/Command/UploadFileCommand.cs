using MediatR;

namespace Yaml.Application.Command;

public class UploadFileCommand : IRequest<IEnumerable<string>>
{
    public IFormFileCollection Files { get; set; }
}

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, IEnumerable<string>>
{
    private readonly ILogger _logger;

    public UploadFileCommandHandler(ILogger<UploadFileCommandHandler> logger)
    {
        _logger = logger;
        
    }

    public async Task<IEnumerable<string>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var files = request.Files;
            if (files == null || files.Count == 0)
            {
                return Enumerable.Empty<string>();
            }

            var uploadDirectory = "uploads";
            if (!Directory.Exists(uploadDirectory))
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
                    var fileName = Path.Combine(uploadDirectory, prefix + "_" + file.FileName);
                    using (var stream = new FileStream(fileName, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        uploadedFiles.Add(fileName);
                    }
                }
            }

            return uploadedFiles;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}