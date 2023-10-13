using System.Globalization;
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
        
        try
        {
            var userLoginResponse = await Mediator.Send(command);
            if (userLoginResponse.IsSuccess)
            {
                return Ok(userLoginResponse);
            }

            string errorMessage = _sharedlocalizer["401unauthorized"];
            return Unauthorized(errorMessage);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}