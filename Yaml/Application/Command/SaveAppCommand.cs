using AutoMapper;
using MediatR;
using Yaml.Domain.Entity;
using Yaml.Infrastructure.Dto;

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
            // foreach (var kv in yamlAppInfoDto.KeyVault.KeyVault ?? Enumerable.Empty<string>())
            // {
            //     var yamlKeyVaultInfo = new YamlKeyVaultInfo()
            //     {
            //         ConfigKey = kv, 
            //         AppId = yamlAppInfo.Id
            //     };
            //     await _context.KeyVaultInfo.AddAsync(yamlKeyVaultInfo, cancellationToken);
            // }

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
                   var clusterDomain = _mapper.Map<YamlClusterDomainInfo>(yamlClusterInfoDto.Domain);
                   clusterDomain.ClusterId = cluster.Id;
                   _context.Domain.Update(clusterDomain);
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
               //  
               //  // Key Vault
               // if (yamlClusterInfoDto.KeyVaultFlag)
               // {
               //     var keyVaultInfoList = yamlClusterInfoDto.KeyVault?.Select(dto => new YamlKeyVaultInfo
               //         {
               //             ClusterId = cluster.Id,
               //             ConfigKey = dto.ConfigKey
               //         }
               //     ).ToList() ?? new List<YamlKeyVaultInfo>();
               //     _context.KeyVaultInfo.UpdateRange(keyVaultInfoList);
               // }
               //
               // // Disk info
               // if (yamlClusterInfoDto.DiskInfoFlag)
               // {
               //     Console.WriteLine(yamlClusterInfoDto.Disk.MountPath.Length);
               //     // TODO yamlClusterInfoDto.Disk
               // }
               
            }
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Save app Info [{}] to DB", yamlAppInfoDto.AppName);
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