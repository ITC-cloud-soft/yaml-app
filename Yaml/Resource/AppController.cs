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
    public async Task<IActionResult> DownloadAppFile(GetAppInfoCommand getAppInfoCommand)
    {
        var yamlAppInfoDto = await Mediator.Send(getAppInfoCommand);
        
        // Serialize the app data to JSON.
        var json = JsonConvert.SerializeObject(yamlAppInfoDto);
        
        // Create a FileContentResult with appropriate headers
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(json);

        var fileContentResult = new FileContentResult(fileBytes, "application/json")
        {
            FileDownloadName = "content.json" // Set the file name
        };

        return fileContentResult;

        // Response.Headers.Add("Content-Disposition", "attachment; filename=content.json");
        // Response.Headers.Add("Content-Type", "application/json"); // Set the content type to JSON

        // Convert the JSON string to bytes.
        // return File(fileBytes, "application/octet-stream"); 
    }
    
    [HttpGet("info/{AppId}")]
    [Produces("application/json")] 
    public async Task<IActionResult> GetAppInfo([FromRoute]GetAppInfoCommand getAppInfoCommand)
    {
        var yamlAppInfoDto = await Mediator.Send(getAppInfoCommand);
        return Ok(yamlAppInfoDto);
    }
}