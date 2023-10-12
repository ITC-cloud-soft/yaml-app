using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace Yaml.Application.Command;

public class UserLoginCommand : IRequest<UserLoginResponse>
{
    
    [StringLength(7, MinimumLength = 1, ErrorMessage = "用户名长度必须在1到50之间")]
    public string? Name { get; set; } 
    public string? Password { get; set; }
}

public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, UserLoginResponse>
{
    private readonly MyDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public UserLoginCommandHandler(MyDbContext context, IMapper mapper, ILogger<UserLoginCommandHandler>  logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserLoginResponse> Handle(UserLoginCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.User.Where(user=>user.Name == command.Name && user.Password == command.Password).SingleOrDefaultAsync(cancellationToken);
            if (user != null)
            {
                return new UserLoginResponse
                {
                    IsSuccess = true,
                    Username = command.Name,
                    UserId = user.Id
                };
            }
            return new UserLoginResponse { IsSuccess = false, ErrorMessage = "login failed try again latter" };
         
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "error");
            return new UserLoginResponse { IsSuccess = false, ErrorMessage = "login failed try again latter" };
        }
    }
}