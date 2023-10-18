using System.Globalization;
using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Yaml;
using Yaml.Application.Command;
using Yaml.Resource;


public class UserController : ApiControllerBase
{
    private readonly IValidator<UserLoginCommand> _validator;
    private readonly IStringLocalizer<UserController> _localizer;
    private readonly IStringLocalizer<SharedResources> _sharedlocalizer;

    public UserController(
        IValidator<UserLoginCommand> validator, 
        IStringLocalizer<UserController> localizer,
        IStringLocalizer<SharedResources> sharedlocalizer)
    {
        _validator = validator;
        _localizer = localizer;
        _sharedlocalizer = sharedlocalizer;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginCommand command)
    {
        var response = await Mediator.Send(command);
        return StatusCode((int)response.Status, response);
    }
}