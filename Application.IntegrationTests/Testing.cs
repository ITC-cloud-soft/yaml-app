
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using MySqlConnector;
using NUnit.Framework;
using Respawn;
using Respawn.Graph;
using Yaml;

namespace YamlTest;

[SetUpFixture]
public class Testing
{
    private static WebApplicationFactory<Program> _factory = null!;
    private static IConfiguration _configuration = null!;
    private static IServiceScopeFactory _scopeFactory = null!;
    private static Respawner _checkpoint = null!;
    private static string? _currentUserId;
    
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        _factory = new CustomWebApplicationFactory();
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        _configuration = _factory.Services.GetRequiredService<IConfiguration>();

        var respawnerOptions =  new RespawnerOptions
        {
            DbAdapter = DbAdapter.MySql,
            SchemasToInclude = new [] {"test_db"},
            TablesToInclude = new Table []
            {
                "wf_flow",
                "tbl_workflow_basic_info", 
                "TBL_WFLOW_APPLYLIST_INFO",
                "TBL_WFLOW_APPLY_CONTENTS"
            }
        };
        MySqlConnection connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")!);
        connection.Open();
        
        _checkpoint = Respawner.CreateAsync(connection, respawnerOptions).GetAwaiter().GetResult();
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task SendAsync(IBaseRequest request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        await mediator.Send(request);
    }
    
    public static async Task ResetState()
    {
        try
        {
            await _checkpoint.ResetAsync(_configuration.GetConnectionString("DefaultConnection")!);
        }
        catch (Exception) 
        {
        }

        _currentUserId = null;
    }

    
}
