using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Yaml.Application;
using Yaml.Application.Command;

namespace Yaml.Resource;

public class AppController : ApiControllerBase
{

    [HttpPost("save")]
    public async Task<ActionResult<string>> Save([FromBody] SaveYamlAppCommand saveYamlAppCommand)
    {
        return await Mediator.Send(saveYamlAppCommand);
    }

    [HttpGet("download")]
    public async Task<IActionResult> DownloadAppFile([FromQuery]GetAppQuery getAppQuery)
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
    
    [HttpGet("{AppId}")]
    [Produces("application/json")] 
    public async Task<IActionResult> GetAppInfo([FromRoute]GetAppQuery getAppQuery)
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
        var files = Request.Form.Files;
        if (files == null || files.Count == 0)
        {
            return BadRequest("没有选择要上传的文件");
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
                var fileName = Path.Combine(uploadDirectory, prefix + "_" + file.FileName) ;
                using (var stream = new FileStream(fileName, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                    uploadedFiles.Add(fileName);
                }
            }
        }
        return Ok(new { message = "文件上传成功", files = uploadedFiles });
    }


    [HttpDelete("deleteItem")]
    public async Task<IActionResult> Delete([FromQuery] DeleteCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
    
}