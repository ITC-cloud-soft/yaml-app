using System.Text.Json;
using MediatR;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.Exception;
using Yaml.Resource;

namespace Yaml.Application.Command;

public class ImportJsonCommand : IRequest<string>
{
    public IFormFile File { get; set; }
}

public class ImportJsonCommandHandler : IRequestHandler<ImportJsonCommand, string> 
{
    private readonly ILogger _logger;
    private readonly ISender? _mediator;
    public ImportJsonCommandHandler(ILogger<ImportJsonCommandHandler> logger, ISender mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<string> Handle(ImportJsonCommand request, CancellationToken cancellationToken)
    {
        var jsonFile = request.File;
        var jsonFileStream = jsonFile.OpenReadStream();
        var reader = new StreamReader(jsonFileStream);
        try
        {
            var fileContent = await reader.ReadToEndAsync(cancellationToken);
            var yamlAppInfoDto = JsonSerializer.Deserialize<YamlAppInfoDto>(fileContent);
            await _mediator.Send(new SaveYamlAppCommand { appInfoDto = yamlAppInfoDto });
            return fileContent;
        }
        catch (Exception e)
        {
            _logger.LogError("yaml file deserialize error", e);
            throw new ServiceException("yaml file deserialize error");
        }
        finally
        {
            await jsonFileStream.DisposeAsync();
            reader.Dispose();
        }
    }
}