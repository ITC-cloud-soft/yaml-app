using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Yaml.Application;

namespace Yaml.Resource;

public class AppController : ApiControllerBase
{

    [HttpPost("save")]
    public async Task<ActionResult<string>> Save(SaveYamlAppCommand saveYamlAppCommand)
    {
        return await Mediator.Send(saveYamlAppCommand);
    }

    [HttpGet("download")]
    public async Task<IActionResult> DownloadAppFile([FromQuery]GetAppInfoQuery getAppInfoQuery)
    {
        var yamlAppInfoDto = await Mediator.Send(getAppInfoQuery);
        
        // Serialize the app data to JSON.
        var json = JsonConvert.SerializeObject(yamlAppInfoDto);
        
        // Create a FileContentResult with appropriate headers
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(json);

        var fileContentResult = new FileContentResult(fileBytes, "application/json")
        {
            FileDownloadName = "content.json" // Set the file name
        };

        return fileContentResult;
    }
    
    [HttpGet("{AppId}")]
    [Produces("application/json")] 
    public async Task<IActionResult> GetAppInfo([FromRoute]GetAppInfoQuery getAppInfoQuery)
    {
        return Ok(await Mediator.Send(getAppInfoQuery));
    }

    [HttpPost("deploy")]
    public async Task<IActionResult> Deploy([FromBody] DeployAppCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
}