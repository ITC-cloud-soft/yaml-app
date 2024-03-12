using AutoMapper;
using MediatR;
using Yaml.Domain.Entity;
using Yaml.Infrastructure.Dto;

namespace Yaml.Application.Command;

public class SaveClusterCommand: IRequest<string>
{
    public YamlClusterInfoDto clusterInfo { get; set; }
}

public class SaveClusterCommandHandler : IRequestHandler<SaveClusterCommand, string>
{
    private readonly MyDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public SaveClusterCommandHandler(MyDbContext context, IMapper imapper, ILogger<SaveYamlAppCommandHandler> logger)
    {
        _context = context;
        _mapper = imapper;
        _logger = logger;
    }

    public async Task<string> Handle(SaveClusterCommand request, CancellationToken cancellationToken)
    {
        // await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken: cancellationToken);

        var yamlClusterInfoDto = request.clusterInfo;
        var cluster = await HandleCluster(yamlClusterInfoDto, cancellationToken);
        
        await HandleDomain(yamlClusterInfoDto, cluster, cancellationToken);
        
        await HandleConfigMap(yamlClusterInfoDto, cluster, cancellationToken);
        
        await HandleConfigFile(yamlClusterInfoDto, cluster, cancellationToken);

        await HandleKeyVault(yamlClusterInfoDto, cluster, cancellationToken);

        await HandleDiskInfo(yamlClusterInfoDto, cluster, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        // await transaction.CommitAsync(cancellationToken);
        _logger.LogInformation("Save cluster Info [{}] to DB", yamlClusterInfoDto.ClusterName);
        return "success";
    }
    
    private async Task<YamlClusterInfo> HandleCluster(YamlClusterInfoDto yamlClusterInfoDto, CancellationToken cancellationToken)
{
    // save cluster 
    var cluster = _mapper.Map<YamlClusterInfo>(yamlClusterInfoDto);
    if (cluster.Id < 0)
    {
        cluster.Id = 0;
    }
    _context.ClusterContext.Update(cluster);
    await _context.SaveChangesAsync(cancellationToken);
    return cluster;
}

    private async Task<string> HandleDomain(YamlClusterInfoDto yamlClusterInfoDto, YamlClusterInfo cluster, CancellationToken cancellationToken)
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
                clusterDomain.ClusterId = cluster.Id;
                await _context.DomainContext.AddAsync(clusterDomain);
            }
            else
            {
                // If exists, update its properties directly
                domainInfo.DomainName = yamlClusterInfoDto.Domain.DomainName;
                domainInfo.Certification = yamlClusterInfoDto.Domain.Certification;
                domainInfo.PrivateKey = yamlClusterInfoDto.Domain.PrivateKey;
            }
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
        }
        return "success";
    }

    private async Task<string> HandleConfigFile(YamlClusterInfoDto yamlClusterInfoDto, YamlClusterInfo cluster, CancellationToken cancellationToken)
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
        }
        return "success";
    }

    private async Task<string> HandleDiskInfo(YamlClusterInfoDto yamlClusterInfoDto, YamlClusterInfo cluster, CancellationToken cancellationToken)
    {
        if (yamlClusterInfoDto.DiskInfoFlag && yamlClusterInfoDto.DiskInfoList != null)
        {
            Random random = new Random();
            var diskInfoList = yamlClusterInfoDto.DiskInfoList.Select(dto =>
            {
                var yamlClusterDiskInfo = _mapper.Map<YamlClusterDiskInfo>(dto);
                yamlClusterDiskInfo.ClusterId = cluster.Id;
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
        }

        return "success";
    }
}