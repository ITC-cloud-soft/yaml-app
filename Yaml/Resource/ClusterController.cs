using Microsoft.AspNetCore.Mvc;
using Yaml.Application.Command;

namespace Yaml.Resource;

public class ClusterController : ApiControllerBase
{
    [HttpPost("save")]
    [Produces("application/json")]
    public async Task<IActionResult> Deploy([FromBody] SaveClusterCommand command)
    {
        return Ok(command);
    }
}