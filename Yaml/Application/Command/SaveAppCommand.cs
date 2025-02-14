using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        try
        {
            var yamlAppInfoDto = command.appInfoDto;
            
            // save app info
            var yamlAppInfo =await HandleAppInfo(yamlAppInfoDto, cancellationToken);

            // save app key vault info
            await HandleKeyVaultApp(yamlAppInfoDto, cancellationToken, yamlAppInfo.Id);

            // save whole cluster info
            foreach (var yamlClusterInfoDto in yamlAppInfoDto.ClusterInfoList ?? Enumerable.Empty<YamlClusterInfoDto>())
            {
                var cluster = await HandleCluster(yamlClusterInfoDto, cancellationToken, yamlAppInfo.Id);
                await HandleDomain(yamlClusterInfoDto, cluster, cancellationToken);
                await HandleConfigMap(yamlClusterInfoDto, cluster, cancellationToken);
                await HandleConfigFile(yamlClusterInfoDto, cluster, cancellationToken);
                await HandleKeyVault(yamlClusterInfoDto, cluster, cancellationToken);
                await HandleDiskInfo(yamlClusterInfoDto, cluster, cancellationToken);
            }

            // commit transaction
            _logger.LogInformation("Save app Info [{}] to DB", yamlAppInfoDto.AppName);
            return "success";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving app Info to DB");
            throw new ServiceException("Error saving app Info to DB", e);
        }
    }
    
    
    private async Task<YamlAppInfo> HandleAppInfo(YamlAppInfoDto yamlAppInfoDto, CancellationToken cancellationToken)
    {
        // save cluster 
        var yamlAppInfo = _mapper.Map<YamlAppInfo>(yamlAppInfoDto);
        var existingAppInfo = await _context.AppInfoContext.FirstOrDefaultAsync();
        if (existingAppInfo != null)
        {
            _context.Entry(existingAppInfo).CurrentValues.SetValues(yamlAppInfo);
        }
        else
        {
            _context.AppInfoContext.Add(yamlAppInfo);
        }
        await _context.SaveChangesAsync(cancellationToken);
        return yamlAppInfo;
    }


    private async Task<YamlClusterInfo> HandleCluster(YamlClusterInfoDto yamlClusterInfoDto, CancellationToken cancellationToken, 
        int appId)
    {
        // set default port
        if (!yamlClusterInfoDto.PortFlag)
        {
            yamlClusterInfoDto.Port = "80";
            yamlClusterInfoDto.TargetPort = "80";
        }
        // save cluster 
        var cluster = _mapper.Map<YamlClusterInfo>(yamlClusterInfoDto);
        cluster.AppId = appId;
        
        // 检查ID值来决定是新增还是更新
        if (cluster.Id <= 0)
        {
            cluster.Id = null;
            // ID为0或负值时新增
            _context.ClusterContext.Add(cluster);
        }
        else
        {
            // ID为正值时更新
            _context.ClusterContext.Update(cluster);
        }

        // 保存更改
        await _context.SaveChangesAsync(cancellationToken);
        return cluster;
    }
    

    private async Task<string> HandleKeyVaultApp(YamlAppInfoDto yamlAppInfoDto, CancellationToken cancellationToken, int appId)
    {
        foreach (var kv in yamlAppInfoDto.KeyVault?.KeyVault ?? Enumerable.Empty<KeyVaultDto>())
        {
            var yamlKeyVaultInfo = new YamlKeyVaultInfo
            {
                ConfigKey = kv.ConfigKey,
                AppId = appId,
                ClusterId = kv.ClusterId,
                Value = kv.Value
            };

            var existingKeyVaultInfo = await _context.KeyVaultInfoContext.FirstOrDefaultAsync(e => e.AppId == appId && e.ConfigKey == kv.ConfigKey, cancellationToken);
            if (existingKeyVaultInfo != null)
            {
                existingKeyVaultInfo.ConfigKey = kv.ConfigKey;
                existingKeyVaultInfo.Value = kv.Value;
                _context.KeyVaultInfoContext.Update(existingKeyVaultInfo);
            }
            else
            {
                await _context.KeyVaultInfoContext.AddAsync(yamlKeyVaultInfo, cancellationToken);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        return "success";
    }

    private async Task<string> HandleDomain(YamlClusterInfoDto yamlClusterInfoDto, YamlClusterInfo cluster,
        CancellationToken cancellationToken)
    {
        if (yamlClusterInfoDto.Domain != null)
        {
            // Check if the domain already exists           
            var domainInfo = _context.DomainContext
                .FirstOrDefault(e => e.ClusterId == cluster.Id);
            if (domainInfo == null)
            {
                // If doesn't exist, map and add a new domain 
                var clusterDomain = _mapper.Map<YamlClusterDomainInfo>(yamlClusterInfoDto.Domain);
                clusterDomain.ClusterId = cluster.Id ?? 0;
                await _context.DomainContext.AddAsync(clusterDomain);
            }
            else
            {
                // If exists, update its properties directly
                domainInfo.DomainName = yamlClusterInfoDto.Domain.DomainName;
                domainInfo.Certification = yamlClusterInfoDto.Domain.Certification;
                domainInfo.PrivateKey = yamlClusterInfoDto.Domain.PrivateKey;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        return "success";
    }
    

    private async Task<string> HandleConfigMap(YamlClusterInfoDto yamlClusterInfoDto, YamlClusterInfo cluster, CancellationToken cancellationToken)
    {
        if (yamlClusterInfoDto.ConfigMapFlag)
        {
            var configMapList = yamlClusterInfoDto.ConfigMap?.Select(dto =>
            {
                var configMap = _mapper.Map<YamlClusterConfigMapInfo>(dto);
                configMap.ClusterId = cluster.Id;
                return configMap;
            }).ToList() ?? new List<YamlClusterConfigMapInfo>();
            var newConfigMaps = configMapList.Where(e => e.Id <= 0).ToList();
            var existedConfigMapFiles = configMapList.Where(e => e.Id > 0).ToList();

            if (existedConfigMapFiles.Any())
            {
                _context.ConfigMap.UpdateRange(existedConfigMapFiles);
            }

            if (newConfigMaps.Any())
            {
                await _context.ConfigMap.AddRangeAsync(newConfigMaps, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        return "success";
    }

    private async Task<string> HandleConfigFile(YamlClusterInfoDto yamlClusterInfoDto, YamlClusterInfo cluster,
        CancellationToken cancellationToken)
    {
        if (yamlClusterInfoDto.ConfigMapFileFlag)
        {
            var configFileList = yamlClusterInfoDto.ConfigFile?.Select(dto =>
            {
                var configFile = _mapper.Map<YamlClusterConfigFileInfo>(dto);
                configFile.ClusterId = cluster.Id;
                return configFile;
            }).ToList() ?? new List<YamlClusterConfigFileInfo>();
            var newConfigFiles = configFileList.Where(e => e.Id <= 0).ToList();
            var existedConfigFileFiles = configFileList.Where(e => e.Id > 0).ToList();

            // update first then insert new
            if (existedConfigFileFiles.Any())
            {
                _context.ConfigFile.UpdateRange(existedConfigFileFiles);
            }

            if (newConfigFiles.Any())
            {
                await _context.ConfigFile.AddRangeAsync(newConfigFiles, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        return "success";
    }

    private async Task<string> HandleKeyVault(YamlClusterInfoDto yamlClusterInfoDto, YamlClusterInfo cluster, CancellationToken cancellationToken)
    {
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
            var existed = keyVaultInfoList.Where(e => e.Id > 0).ToList();
            var newKv = keyVaultInfoList.Where(e => e.Id <= 0).ToList();
            if (existed.Any())
            {
                _context.KeyVaultInfoContext.UpdateRange(existed);
            }

            if (newKv.Any())
            {
                foreach (var yamlKeyVaultInfo in newKv)
                {
                    yamlKeyVaultInfo.Id = 0;
                }
                await _context.KeyVaultInfoContext.AddRangeAsync(newKv, cancellationToken);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
        return "success";
    }

    private async Task<string> HandleDiskInfo(YamlClusterInfoDto yamlClusterInfoDto, YamlClusterInfo cluster,
        CancellationToken cancellationToken)
    {
        if (yamlClusterInfoDto.DiskInfoFlag && yamlClusterInfoDto.DiskInfoList != null)
        {
            Random random = new Random();
            var diskInfoList = yamlClusterInfoDto.DiskInfoList.Select(dto =>
            {
                var yamlClusterDiskInfo = _mapper.Map<YamlClusterDiskInfo>(dto);
                yamlClusterDiskInfo.ClusterId = cluster.Id ??  0;
                yamlClusterDiskInfo.Name = cluster.ClusterName + "-" + random.Next(0, 1000);
                return yamlClusterDiskInfo;
            }).ToList();
            var existed = diskInfoList.Where(e => e.Id > 0).ToList();
            var newList = diskInfoList.Where(e => e.Id <= 0).ToList();
            if (existed.Any())
            {
                _context.DiskInfoContext.UpdateRange(existed);
            }

            if (newList.Any())
            {
                await _context.DiskInfoContext.AddRangeAsync(newList, cancellationToken);
            } 
            await _context.SaveChangesAsync(cancellationToken);
        }

        return "success";
    }
}