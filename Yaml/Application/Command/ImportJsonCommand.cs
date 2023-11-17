using System.Text.Json;
using MediatR;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.Exception;

namespace Yaml.Application.Command;

public class ImportJsonCommand : IRequest<string>
{
    public IFormFile File { get; set; }
}

public class ImportJsonCommandHandler : IRequestHandler<ImportJsonCommand, string>
{
    private readonly ILogger _logger;

    public ImportJsonCommandHandler(ILogger<ImportJsonCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<string> Handle(ImportJsonCommand request, CancellationToken cancellationToken)
    {
        var jsonFile = request.File;
        var jsonFileStream = jsonFile.OpenReadStream();
        var reader = new StreamReader(jsonFileStream);
        try
        {
            var fileContent = await reader.ReadToEndAsync();
            var yamlAppInfoDto = JsonSerializer.Deserialize<YamlAppInfoDto>(fileContent);
            Console.WriteLine(yamlAppInfoDto.AppName);
            _logger.LogTrace("yaml file deserialize error");
            return fileContent;
        }
        catch (Exception e)
        {
            _logger.LogError("yaml file deserialize error", e);
            throw new ServiceException("yaml file deserialize error");
        }
        finally
        {
            jsonFileStream.Dispose();
            reader.Dispose();
        }
    }
}