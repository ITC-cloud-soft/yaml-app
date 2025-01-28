using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Yaml.Application.Command;
using Yaml.Application.Query;

namespace Yaml.Resource;

public class AppController : ApiControllerBase
{
    [HttpGet("download/json")]
    public async Task<IActionResult> DownloadAppJsonFile([FromQuery] GetAppQuery getAppQuery)
    {
        var yamlAppInfoDto = await Mediator.Send(getAppQuery);

        yamlAppInfoDto.KubeConfig = "";
        if (yamlAppInfoDto.ClusterInfoList?.Count > 0)
        {
            foreach (var cluster in yamlAppInfoDto.ClusterInfoList)
            {
                cluster.Id = 0;

                if (cluster.Domain != null)
                {
                    cluster.Domain = null;
                }
            
                if (cluster.ConfigFile != null)
                {
                    // can not export config file 
                    cluster.ConfigFile = [];
                }
            
                if (cluster.ConfigMap?.Count > 0)
                {
                    foreach (var configMap in cluster.ConfigMap)
                    {
                        configMap.Id = 0;
                    } 
                }

                if (cluster.KeyVaultFlag && cluster.KeyVault?.Count > 0)
                {
                    foreach (var keyVault in cluster.KeyVault)
                    {
                        keyVault.ClusterId = 0;
                        keyVault.Id = 0;
                    }
                }
                
                if (cluster.DiskInfoList?.Count > 0)
                {
                    foreach (var disk in cluster.DiskInfoList)
                    {
                        disk.Id = 0;
                        disk.ClusterId = 0;
                    }  
                }
            } 
        }
        
        if (yamlAppInfoDto.KeyVaultFlag && yamlAppInfoDto.KeyVault?.KeyVault?.Count >  0)
        {
            foreach (var keyVault in yamlAppInfoDto.KeyVault?.KeyVault)
            {
                keyVault.ClusterId = 0;
                keyVault.Id = 0;
            }
        }

        // Serialize the app data to JSON.
        var json = JsonConvert.SerializeObject(yamlAppInfoDto, Formatting.Indented);
        
        // Create a FileContentResult with appropriate headers
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(json);
        var fileContentResult = new FileContentResult(fileBytes, "application/json")
        {
            FileDownloadName = "content.json" 
        };
        return  fileContentResult;
    }
    
    [HttpGet("download/yaml")]
    public async Task<IActionResult> DownloadAppJsonFile([FromQuery] DownloadYamlFileCommand downloadCommand)
    {
        return await Mediator.Send(downloadCommand);
    }
    
    [HttpGet("info")]
    [Produces("application/json")] 
    public async Task<IActionResult> GetAppInfo([FromQuery] GetAppQuery getAppQuery)
    {
        return Ok(await Mediator.Send(getAppQuery));
    }
    
    [HttpGet("list")]
    [Produces("application/json")] 
    public async Task<IActionResult> GetAppInfo([FromQuery] ListQuery getAppQuery)
    {
        return Ok(await Mediator.Send(getAppQuery));
    }
    
    [HttpPost("deploy")]
    [Produces("application/json")] 
    public async Task<IActionResult> Deploy([FromBody] DeployAppCommand command)
    {
        if (command.AppInfoDto.K8sConfig)
        {
            var app = await Mediator.Send(new GetAppQuery { AppId = command.AppInfoDto.Id, UserId = 1 });
            command.AppInfoDto = app;
            return Ok(await Mediator.Send(command));
        }

        var deployByYamlCommand = new DeployByYamlCommand(){AppId = command.AppInfoDto.Id};
        await Mediator.Send(deployByYamlCommand);
        return Ok();
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> Deploy()
    {
        return Ok(await Mediator.Send(new UploadFileCommand{Files =  Request.Form.Files}));
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] DeleteCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("import/json")]
    public async Task<IActionResult> ImportJSon()
    {
        return Ok(await Mediator.Send(new UploadJsonFileCommand{File = Request.Form.Files[0]}));
    }
    
    [HttpPost("save")]
    public async Task<ActionResult<string>> Save([FromBody] SaveYamlAppCommand saveYamlAppCommand)
    {
        return Ok(await Mediator.Send(saveYamlAppCommand));
    }
    
    [HttpDelete("withDraw")]
    public async Task<IActionResult> Delete([FromQuery] WithdrawClusterCommand command)
    {
        await Mediator.Send(command);
        return Ok();
    }
}