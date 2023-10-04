using Yaml.Application;
namespace Yaml.Resource;

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class MyController : ApiControllerBase
{

    private readonly MyDbContext _myDbContext;
    private readonly IMapper _mapper;
    private readonly IValidator<SaveYamlAppCommand> _validator;

    public MyController(MyDbContext myDbContext, IMapper mapper, IMediator mediator,
        IValidator<SaveYamlAppCommand> validator)
    {
        _myDbContext = myDbContext;
        _mapper = mapper;
        _validator = validator;
    }

    [HttpGet("list3")]
    public async Task<ActionResult<string>> GetAppInfo([FromBody] [Required] SaveYamlAppCommand command)
    {
        // var validationResult = _validator.Validate(command);
        //
        // if (!validationResult.IsValid)
        // {
        //     // 验证失败，处理错误
        //     var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
        //     return BadRequest(errors);
        // }
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(command, new ValidationContext(command), results, true);

        if (!isValid)
        {
            // 模型验证失败，处理错误
            var errors = results.Select(r => r.ErrorMessage);
            return BadRequest(errors);
        }

        return await Mediator.Send(command);

    }
}
