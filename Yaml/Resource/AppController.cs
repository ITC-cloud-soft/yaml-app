using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Yaml.Application.Command;
using Yaml.Application.Query;
using Yaml.Infrastructure.Dto;

namespace Yaml.Resource;

public class AppController : ApiControllerBase
{
    [HttpGet("download")]
    public async Task<IActionResult> DownloadAppFile([FromQuery] GetAppQuery getAppQuery)
    {
        var yamlAppInfoDto = await Mediator.Send(getAppQuery);
        
        // Serialize the app data to JSON.
        var json = JsonConvert.SerializeObject(yamlAppInfoDto);
        
        // Create a FileContentResult with appropriate headers
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(json);
        var fileContentResult = new FileContentResult(fileBytes, "application/json")
        {
            FileDownloadName = "content.json" 
        };
        return fileContentResult;
    }
    
    [HttpGet("get")]
    [Produces("application/json")] 
    public async Task<IActionResult> GetAppInfo([FromQuery] GetAppQuery getAppQuery)
    {
        return Ok(await Mediator.Send(getAppQuery));
    }

    [HttpPost("deploy")]
    [Produces("application/json")] 
    public async Task<IActionResult> Deploy([FromBody] DeployAppCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
    
    [HttpPost("uploadFiles")]
    public async Task<IActionResult> Deploy()
    {
        return Ok(await Mediator.Send(new UploadFileCommand{Files =  Request.Form.Files}));
    }

    [HttpDelete("deleteItem")]
    public async Task<IActionResult> Delete([FromQuery] DeleteCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("importJson")]
    public async Task<IActionResult> ImportJSon()
    {
        return Ok(await Mediator.Send(new ImportJsonCommand{File = Request.Form.Files[0]}));
    }
    
    [HttpPost("save")]
    public async Task<ActionResult<string>> Save([FromBody] SaveYamlAppCommand saveYamlAppCommand)
    {
        return Ok(await Mediator.Send(saveYamlAppCommand));
    }
}