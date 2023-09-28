using RazorLight;
using Yaml.Application;
using Yaml.Domain.Entity;
using Yaml.Infrastructure.Dto;
using Yaml.Resource.Vo;

namespace Yaml.Resource;

using System.ComponentModel.DataAnnotations;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RazorEngine;
using RazorEngine.Templating;



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

    [HttpGet("list2")]
    public IActionResult List()
    {
        var y = new YamlAppInfo()
        {
            AppName = "jame",
            Cr = "CR",
            Token = "1231231",
            MailAddress = "@com",
            KeyVaultFlag = true,
            Tenantid = "123",
            KeyVaultName = "name",
            ManagedId = "manana",
            NetdataFlag = true
        };

        _myDbContext.AppInfoContext.Add(y);
        _myDbContext.SaveChanges();
        var yamlAppInfoDtoList = _myDbContext.AppInfoContext
            .AsNoTracking()
            .ProjectTo<YamlAppInfoDto>(_mapper.ConfigurationProvider)
            .ToList();
        var yamlAppInfos = _myDbContext.AppInfoContext.ToList();

        Console.WriteLine(yamlAppInfos[0].ToString());
        return Ok(yamlAppInfoDtoList);
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

    [HttpGet("generateYaml")]
    public async Task<ActionResult<string>> GenerateYamlFile()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var path = Path.Combine(currentDirectory, "YamlFile/YamlTemplate.cshtml");
        var template = System.IO.File.ReadAllText("/Users/yanzou/RiderProjects/YamlController/Yaml/YamlFile/YamlTemplate.cshtml");
        // 创建一个 RazorLight 引擎实例
        var engine = new RazorLightEngineBuilder()
            .UseFileSystemProject("/Users/yanzou/RiderProjects/YamlController/Yaml/YamlFile/")
            .UseMemoryCachingProvider()
            .Build();
        
        
        // 创建数据模型
        var model = new YamlDataModel
        {
            Title = "My YAML Data",
            Date = DateTime.UtcNow,
            Item1 = "Value 1",
            Item2 = "Value 2"
        };
        var renderedTemplate = Engine.Razor.RunCompile(template, "templateKey", null, model);

        Console.WriteLine(renderedTemplate);
        // 渲染模板
        var result = await engine.CompileRenderAsync("/Users/yanzou/RiderProjects/YamlController/Yaml/YamlFile/YamlTemplate.cshtml", model);

        // 输出生成的 YAML 内容
        Console.WriteLine(result);

        return ";";
    }
    

    [HttpPost("generateYamlByJsonStr")]
    public IActionResult GenerateYamlByJsonStr(AppInfoVo appInfoVo)
    {
        var serializeObject = JsonConvert.SerializeObject(appInfoVo);
        return Ok(serializeObject);
    }
    
    [HttpPost("generateYamlByJsonStr1")]
    public IActionResult GenerateYamlByJsonStr1(ConfigMapDto clusterInfo)
    {
        var serializeObject = JsonConvert.SerializeObject(clusterInfo);
        return Ok(serializeObject);
    }
}

public class YamlDataModel
{
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public string Item1 { get; set; }
    public string Item2 { get; set; }
}
