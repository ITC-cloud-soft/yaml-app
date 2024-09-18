using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yaml.Infrastructure.Dto;

namespace Yaml.Application.Query;

public class ListQuery: IRequest<List<YamlAppInfoDto>>
{
    
}

public class ListAppInfoCommandHandler : IRequestHandler<ListQuery, List<YamlAppInfoDto>>
{
    private readonly MyDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;


    public ListAppInfoCommandHandler(MyDbContext context, IMapper mapper, ILogger<ListAppInfoCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<YamlAppInfoDto>> Handle(ListQuery query, CancellationToken cancellationToken)
    {

        try
        {
            // app info
            var yamlAppInfoDto = await _context.AppInfoContext
                .AsTracking()
                .ProjectTo<YamlAppInfoDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return yamlAppInfoDto;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Details: ");
            throw;
        }
    }
}
