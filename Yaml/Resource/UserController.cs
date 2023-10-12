using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Yaml.Application.Command;

namespace Yaml.Resource;

public class UserController : ApiControllerBase
{
    private readonly IValidator<UserLoginCommand> _validator;


    public UserController(IValidator<UserLoginCommand> validator)
    {
        _validator = validator;
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
            return Unauthorized();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
}