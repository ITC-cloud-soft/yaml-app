using AutoMapper;
using MediatR;
using Yaml.Domain.Entity;
using Yaml.Infrastructure.Dto;

namespace Yaml.Application;

public class SaveYamlAppCommand : IRequest<string>
{
    public YamlAppInfoDto AppInfo { get; set; }
}

public class SaveYamlAppCommandHandler : IRequestHandler<SaveYamlAppCommand, string>
{
    private readonly MyDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public SaveYamlAppCommandHandler(MyDbContext context, IMapper imapper, ILogger<SaveYamlAppCommandHandler> logger)
    {
        _context = context;
        _mapper = imapper;
        _logger = logger;
    }

    public async Task<string> Handle(SaveYamlAppCommand command, CancellationToken cancellationToken)
    {
       await using var transaction =  await _context.Database.BeginTransactionAsync(cancellationToken: cancellationToken);
        try
        {
            var yamlAppInfoDto = command.AppInfo;
                
            // save app info
            var yamlAppInfo = _mapper.Map<YamlAppInfo>(yamlAppInfoDto);
            await _context.AppInfoContext.AddAsync(yamlAppInfo, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // save cluster info
            foreach (var yamlClusterInfoDto in yamlAppInfoDto.ClusterInfoList)
            {
                // cluster 
                var cluster = _mapper.Map<YamlClusterInfo>(yamlClusterInfoDto);
                cluster.AppId = yamlAppInfo.Id;
                await _context.ClusterContext.AddAsync(cluster, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                    
                // domain
                var  domainInfos = yamlClusterInfoDto.DomainList.Select(domainDto =>
                {
                    var clusterDomain = _mapper.Map<YamlClusterDomainInfo>(domainDto);
                    clusterDomain.ClusterId = cluster.Id;
                    return clusterDomain;
                }).ToList();
                await _context.Domain.AddRangeAsync(domainInfos, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                    
                // ConfigMap
                var configMapList = yamlClusterInfoDto.ConfigMap?.Select(dto =>
                {
                    var configMap = _mapper.Map<YamlClusterConfigMapInfo>(dto);
                    configMap.ClusterId = cluster.Id;
                    return configMap;
                }).ToList();
                await _context.ConfigMap.AddRangeAsync(configMapList, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // ConfigMapFile
                var configFileList = yamlClusterInfoDto.ConfigFile?.Select(dto =>
                {
                    var configFile = _mapper.Map<YamlClusterConfigFileInfo>(dto);
                    configFile.ClusterId = cluster.Id;
                    return configFile;
                }).ToList();
                await _context.ConfigFile.AddRangeAsync(configFileList, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
                
            _logger.LogInformation("Save app Info [{}] to DB", yamlAppInfoDto.AppName);
            await transaction.CommitAsync(cancellationToken);
            return "success";
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(e, "Error saving app Info to DB");
            return "error";
        }
    }
}