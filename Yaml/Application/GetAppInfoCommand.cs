using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Yaml.Domain.Entity;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.Exception;

namespace Yaml.Application;

public class GetAppInfoCommand : IRequest<YamlAppInfoDto>
{
    public int AppId { get; set; }
}

public class GetAppInfoCommandHandler : IRequestHandler<GetAppInfoCommand, YamlAppInfoDto>
{
    private readonly MyDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;
    

    public GetAppInfoCommandHandler(MyDbContext context, IMapper mapper, ILogger<GetAppInfoCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<YamlAppInfoDto> Handle(GetAppInfoCommand command, CancellationToken cancellationToken)
    {
        // app info
        var appInfo = await _context.AppInfoContext.Where(e => e.Id==command.AppId).FirstOrDefaultAsync(cancellationToken);
        if (appInfo == null)
        {
            throw new NotFoundException($"App [{command.AppId}] info not found");
        }
        var yamlAppInfoDto = _mapper.Map<YamlAppInfoDto>(appInfo);
        yamlAppInfoDto.KeyVault = new AppKeyVault()
        {
            TenantId = appInfo.ManagedId,
            KeyVaultName = appInfo.KeyVaultName,
            ManagedId = appInfo.ManagedId,
            KeyVault = null // TODO
        };
        
        // cluster 
        List<YamlClusterInfo> yamlClusterInfoList =
            await _context.ClusterContext.Where(e => e.AppId == appInfo.Id).ToListAsync(cancellationToken);

        List<YamlClusterInfoDto> yamlClusterInfoDtoList = new List<YamlClusterInfoDto>();
        foreach (var yamlClusterInfo in yamlClusterInfoList)
        {
            var yamlClusterInfoDto = _mapper.Map<YamlClusterInfoDto>(yamlClusterInfo);

            // Domain
            var yamlClusterDomainInfoList = await _context.Domain.Where(e => e.ClusterId == yamlClusterInfo.Id).ToListAsync(cancellationToken);
            yamlClusterInfoDto.DomainList = _mapper.Map<List<DomainDto>>(yamlClusterDomainInfoList);;

            // ConfigMap
            var yamlClusterConfigMapInfoList = await _context.ConfigMap.Where(e => e.ClusterId == yamlClusterInfo.Id).ToListAsync(cancellationToken);
            yamlClusterInfoDto.ConfigMap = _mapper.Map<List<ConfigMapDto>>(yamlClusterConfigMapInfoList);
        
            // ConfigMap
            var yamlClusterConfigFileInfoList = await _context.ConfigFile.Where(e => e.ClusterId == yamlClusterInfo.Id).ToListAsync(cancellationToken);
            yamlClusterInfoDto.ConfigFile= _mapper.Map<List<ConfigFileDto>>(yamlClusterConfigFileInfoList);
        
            yamlClusterInfoDtoList.Add(yamlClusterInfoDto);
            
            _logger.LogInformation("Get App:[{}] and AppId:[{}] info from DB", appInfo.AppName, appInfo.Id);
        }

        yamlAppInfoDto.ClusterInfoList = yamlClusterInfoDtoList;
        return yamlAppInfoDto;
    }
}