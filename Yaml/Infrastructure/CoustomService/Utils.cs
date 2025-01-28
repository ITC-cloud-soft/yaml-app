using System.Collections.ObjectModel;
using Yaml.Infrastructure.Dto;

namespace Yaml.Infrastructure.CoustomService;

public class Utils
{
    private readonly IConfiguration _configuration;

    public Utils(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task UpdateClusterConfigFiel(YamlClusterInfoDto dto)
    {

        foreach (var configFile in dto.ConfigFile ?? new List<ConfigFileDto>())
        {
            var uploadDirectory = _configuration.GetConnectionString("UploadDirectory");
            var configFilePath = Path.Combine(uploadDirectory, configFile.FileLink);
            var fileContent = await File.ReadAllTextAsync(configFilePath);
            var fileName = configFile.FileLink.Split("_")[1];

            configFile.FileLink = fileName;
            configFile.FileContent = fileContent;
        }
    }
}