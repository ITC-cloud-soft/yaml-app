using Microsoft.AspNetCore.Mvc;
using Yaml.Application.Query;

namespace Yaml.Resource;

public class DownloadFileController : ApiControllerBase
{
    
    [HttpGet("{fileName}")]
    public async Task<IActionResult> Save([FromRoute] string fileName)
    {
       return await Mediator.Send(new DownloadManualCommand{FileName = fileName});
    }
}