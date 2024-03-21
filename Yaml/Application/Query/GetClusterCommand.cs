using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.Exception;

namespace Yaml.Application.Query;

public class GetClusterCommand: IRequest<YamlClusterInfoDto>
{
    public int ClusterId { get; set; }
}

public class GetClusterCommandHandler : IRequestHandler<GetClusterCommand, YamlClusterInfoDto>
{
    
    private readonly MyDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public GetClusterCommandHandler(MyDbContext context, IMapper mapper, ILogger<GetClusterCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<YamlClusterInfoDto> Handle(GetClusterCommand request, CancellationToken cancellationToken)
    {
      
        _logger.LogInformation("Query cluster [{ClusterId}]", request.ClusterId);
        var cluster = _context.ClusterContext
            .AsNoTracking()
            .Where(e => e.Id == request.ClusterId)
            .ProjectTo<YamlClusterInfoDto>(_mapper.ConfigurationProvider)
            .SingleOrDefault();
        
        if (cluster == null)
        {
            throw new NotFoundException($"Cluster [{request.ClusterId}] info not found");
        }
        
        cluster.Domain = await _context.DomainContext
            .AsNoTracking()
            .Where(e => e.ClusterId == request.ClusterId)
            .ProjectTo<DomainDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        cluster.ConfigMap = await _context.ConfigMap
            .AsNoTracking()
            .Where(e => e.ClusterId == request.ClusterId)
            .ProjectTo<ConfigMapDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        cluster.ConfigFile = await _context.ConfigFile
            .AsNoTracking()
            .Where(e => e.ClusterId == request.ClusterId)
            .ProjectTo<ConfigFileDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        cluster.KeyVault = await _context.KeyVaultInfoContext
            .AsNoTracking()
            .Where(e => e.ClusterId == request.ClusterId)
            .ProjectTo<KeyVaultDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        cluster.DiskInfoList = await _context.DiskInfoContext
            .AsNoTracking()
            .Where(e => e.ClusterId == request.ClusterId)
            .ProjectTo<DiskInfoDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
                
        // set app name in disk info
        // cluster.DiskInfoList = cluster.DiskInfoList.Select(diskInfoDto =>
        // {
        //     diskInfoDto.AppName = cluster.AppName;
        //     return diskInfoDto;
        // }).ToList();
        return cluster;
    }
}