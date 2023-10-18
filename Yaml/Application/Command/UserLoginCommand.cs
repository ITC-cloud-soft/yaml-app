using System.ComponentModel.DataAnnotations;
using System.Net;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Yaml.Application.Response;

namespace Yaml.Application.Command;

public class UserLoginCommand : IRequest<ApiResponse<UserLoginResponse>>
{
    
    [Required(ErrorMessage = "UserNameIsRequired")]
    public string? Name { get; set; } 
    
    [Required(ErrorMessage = "UserNameIsRequired")]
    public string? Password { get; set; }
}

public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, ApiResponse<UserLoginResponse>>
{
    private readonly MyDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;
    private readonly IStringLocalizer<SharedResources> _sharedLocalizer;
    public UserLoginCommandHandler(MyDbContext context,
        IMapper mapper,
        ILogger<UserLoginCommandHandler> logger,
        IStringLocalizer<SharedResources> sharedLocalizer
        )
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _sharedLocalizer = sharedLocalizer;
    }

    public async Task<ApiResponse<UserLoginResponse>> Handle(UserLoginCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.User.Where(user=>user.Name == command.Name && user.Password == command.Password).SingleOrDefaultAsync(cancellationToken);
            if (user != null)
            {
                return new ApiResponse<UserLoginResponse>()
                {
                    Status = HttpStatusCode.OK,
                    Data = new UserLoginResponse
                    {
                        IsSuccess = true,
                        Username = command.Name,
                        UserId = user.Id
                    }
                };
            }
          
            return new ApiResponse<UserLoginResponse>()
            {
                Status = HttpStatusCode.Unauthorized,
                Title =  _sharedLocalizer["401unauthorized"]
            };

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error");
             return new ApiResponse<UserLoginResponse>()
            {
                Status = HttpStatusCode.BadRequest,
                Title = "login failed try again latter"
            };
        }
    }
}