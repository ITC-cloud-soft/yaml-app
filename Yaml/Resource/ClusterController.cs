using Microsoft.AspNetCore.Mvc;
using Yaml.Application.Command;
using Yaml.Application.Query;

namespace Yaml.Resource;

public class ClusterController : ApiControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    public async Task<IActionResult> Deploy([FromBody] SaveClusterCommand command)
    {
        return Ok(command);
    }
    
    [HttpGet("{clusterId}")]
    [Produces("application/json")]
    public async Task<IActionResult> Save([FromRoute] int clusterId)
    {
        return Ok(await Mediator.Send(new GetClusterCommand{ClusterId = clusterId}));
    }
}