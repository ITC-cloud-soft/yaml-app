using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yaml.Domain.Entity;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.Exception;

namespace Yaml.Application;

public class GetAppQuery : IRequest<YamlAppInfoDto>
{
    public int AppId { get; set; }
}

public class GetAppInfoCommandHandler : IRequestHandler<GetAppQuery, YamlAppInfoDto>
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

    public async Task<YamlAppInfoDto> Handle(GetAppQuery query, CancellationToken cancellationToken)
    {
  
        try
        {
            // app info
            var appInfo = await _context.AppInfoContext.Where(e => e.Id == query.AppId)
                .FirstOrDefaultAsync(cancellationToken);

            if (appInfo == null)
            {
                throw new NotFoundException($"App [{query.AppId}] info not found");
            }

            var yamlAppInfoDto = _mapper.Map<YamlAppInfoDto>(appInfo);

            // Set app key vault 
            var yamlKeyVaultInfo = await _context.KeyVaultInfo
                .AsNoTracking()
                .Where(e => e.AppId == appInfo.Id)
                .ProjectTo<KeyVaultDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            yamlAppInfoDto.KeyVault = new AppKeyVault(
                tenantId: appInfo.Tenantid,
                keyVaultName: appInfo.KeyVaultName,
                managedId: appInfo.ManagedId,
                keyVault: yamlKeyVaultInfo
            );

            // Get correspond cluster from DB
            List<YamlClusterInfo> yamlClusterInfoList =
                await _context.ClusterContext
                    .AsNoTracking()
                    .Where(e => e.AppId == appInfo.Id)
                    .ToListAsync(cancellationToken);

            // Load Cluster info
            List<YamlClusterInfoDto> yamlClusterInfoDtoList = new List<YamlClusterInfoDto>();
            foreach (var yamlClusterInfo in yamlClusterInfoList)
            {
                var yamlClusterInfoDto = _mapper.Map<YamlClusterInfoDto>(yamlClusterInfo);

                yamlClusterInfoDto.Domain = await _context.Domain
                    .AsNoTracking()
                    .Where(e => e.ClusterId == yamlClusterInfo.Id)
                    .ProjectTo<DomainDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                yamlClusterInfoDto.ConfigMap = await _context.ConfigMap
                    .AsNoTracking()
                    .Where(e => e.ClusterId == yamlClusterInfo.Id)
                    .ProjectTo<ConfigMapDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                yamlClusterInfoDto.ConfigFile = await _context.ConfigFile
                    .AsNoTracking()
                    .Where(e => e.ClusterId == yamlClusterInfo.Id)
                    .ProjectTo<ConfigFileDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                yamlClusterInfoDto.KeyVault = await _context.KeyVaultInfo
                    .AsNoTracking()
                    .Where(e => e.ClusterId == yamlClusterInfo.Id)
                    .ProjectTo<KeyVaultDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                yamlClusterInfoDtoList.Add(yamlClusterInfoDto);
                _logger.LogInformation("Get App:[{}] and AppId:[{}] info from DB", appInfo.AppName, appInfo.Id);
            }

            yamlAppInfoDto.ClusterInfoList = yamlClusterInfoDtoList;
            return yamlAppInfoDto;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Details: ");
            throw;
        }
    }
}