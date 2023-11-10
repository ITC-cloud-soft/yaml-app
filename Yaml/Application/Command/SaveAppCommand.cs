using AutoMapper;
using MediatR;
using Yaml.Domain.Entity;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.Exception;

namespace Yaml.Application.Command;

public class SaveYamlAppCommand : IRequest<string>
{
    public YamlAppInfoDto appInfoDto { get; set; }
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
            var yamlAppInfoDto = command.appInfoDto;
                
            // save app info
            var yamlAppInfo = _mapper.Map<YamlAppInfo>(yamlAppInfoDto);
            _context.AppInfoContext.Update(yamlAppInfo);
            await _context.SaveChangesAsync(cancellationToken);

            // save app key vault info
            foreach (var kv in yamlAppInfoDto.KeyVault.KeyVault ?? Enumerable.Empty<KeyVaultDto>())
            {
                var yamlKeyVaultInfo = new YamlKeyVaultInfo()
                {
                    ConfigKey = kv.ConfigKey, 
                    AppId = yamlAppInfo.Id,
                    Id = kv.Id,
                    Value = kv.Value
                }; 
                _context.KeyVaultInfoContext.Update(yamlKeyVaultInfo);
            }

            // save whole cluster info
            foreach (var yamlClusterInfoDto in yamlAppInfoDto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
            {
                // save cluster 
                var cluster = _mapper.Map<YamlClusterInfo>(yamlClusterInfoDto);
                cluster.AppId = yamlAppInfo.Id;
                if (cluster.Id < 0)
                {
                    cluster.Id = 0;
                }
                _context.ClusterContext.Update(cluster);
                await _context.SaveChangesAsync(cancellationToken);
                
                // save domain 
               if (yamlClusterInfoDto.Domain != null )
               {
                 
                    // Check if the domain already exists           
                   var domainInfo = _context.DomainContext
                       .FirstOrDefault(e => e.ClusterId == cluster.Id);
                   if (domainInfo == null)
                   {
                       // If it doesn't exist, map and add a new domain 
                       var clusterDomain = _mapper.Map<YamlClusterDomainInfo>(yamlClusterInfoDto.Domain);
                       clusterDomain.ClusterId = cluster.Id;
                       _context.DomainContext.Add(clusterDomain);
                   }
                   else
                   {
                       // If it exists, update its properties directly
                       domainInfo.DomainName = yamlClusterInfoDto.Domain.DomainName;
                       domainInfo.Certification = yamlClusterInfoDto.Domain.Certification;
                       domainInfo.PrivateKey = yamlClusterInfoDto.Domain.PrivateKey;
                   }
               }
               
               // save ConfigMap
               if (yamlClusterInfoDto.ConfigMapFlag)
               {
                   var configMapList = yamlClusterInfoDto.ConfigMap?.Select(dto =>
                   {
                       var configMap = _mapper.Map<YamlClusterConfigMapInfo>(dto);
                       configMap.ClusterId = cluster.Id;
                       return configMap;
                   }).ToList() ?? new List<YamlClusterConfigMapInfo>(); 
                   _context.ConfigMap.UpdateRange(configMapList);
               }
               
               // save ConfigMapFile
               if (yamlClusterInfoDto.ConfigMapFileFlag)
               {
                   var configFileList = yamlClusterInfoDto.ConfigFile?.Select(dto =>
                   {
                       var configFile = _mapper.Map<YamlClusterConfigFileInfo>(dto);
                       configFile.ClusterId = cluster.Id;
                       return configFile;
                   }).ToList() ?? new List<YamlClusterConfigFileInfo>();
                   _context.ConfigFile.UpdateRange(configFileList);
               }
                
                // save KeyVault
               if (yamlClusterInfoDto.KeyVaultFlag)
               {
                   var keyVaultInfoList = yamlClusterInfoDto.KeyVault?.Select(dto => new YamlKeyVaultInfo
                       {
                           ClusterId = cluster.Id,
                           ConfigKey = dto.ConfigKey,
                           Id = dto.Id,
                           Value = dto.Value
                       }
                   ).ToList() ?? new List<YamlKeyVaultInfo>();
                   _context.KeyVaultInfoContext.UpdateRange(keyVaultInfoList);
               }
               
               // Disk info
               if (yamlClusterInfoDto.DiskInfoFlag)
               {
                   // TODO save yamlClusterInfoDto.Disk
               }
            }
            
            // commit transaction
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Save app Info [{}] to DB", yamlAppInfoDto.AppName);
            return "success";
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(e, "Error saving app Info to DB");
            throw new ServiceException("Error saving app Info to DB", e);
        }
    }
}