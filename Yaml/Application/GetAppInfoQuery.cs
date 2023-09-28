using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yaml.Domain.Entity;
using Yaml.Infrastructure.Dto;
using Yaml.Infrastructure.Exception;
using Yaml.Resource.Vo;

namespace Yaml.Application;

public record GetAppInfoQuery : IRequest<AppInfoVo>
{
    public int id { get; init; }
}

public class GetAppInfoQueryHandler : IRequestHandler<GetAppInfoQuery, AppInfoVo>
{
    private readonly MyDbContext _context;
    private readonly IMapper _mapper;

    public GetAppInfoQueryHandler(MyDbContext context, IMapper imapper)
    {
        _context = context;
        _mapper = imapper;
    }

    public async Task<AppInfoVo> Handle(GetAppInfoQuery request, CancellationToken cancellationToken)
    {
        var appInfo = await _context.AppInfoContext
            .AsNoTracking()
            .Where(e => e.Id == request.id)
            .ProjectTo<YamlAppInfoDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        var clusterConfig = new ClusterConfig
        {
            Value = "ConfigValueName"
        };

        var newInfo = _mapper.Map(clusterConfig, appInfo);

        Console.Write(appInfo);

        if (appInfo != null)
        {
            return _mapper.Map<AppInfoVo>(newInfo);
        }
        throw new NotFoundException("not found");
    }
}
