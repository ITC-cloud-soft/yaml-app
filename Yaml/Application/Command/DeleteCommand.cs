using MediatR;
using Yaml.Infrastructure.Exception;


namespace Yaml.Application.Command;


public class DeleteCommand:IRequest<string>
{
    public DeleteType Type  { get; set; }
    public int Id { get; set; }
}

public enum DeleteType
{
    Cluster, Domain, ConfigMap, ConfigFile, AppKeyVault, DiskInfo, ClusterKeyVault
}

public class DeleteCommandHandler : IRequestHandler<DeleteCommand, string>
{
    private readonly MyDbContext _context;
    private readonly ILogger _logger;

    public DeleteCommandHandler(MyDbContext context, ILogger<DeleteCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> Handle(DeleteCommand command, CancellationToken cancellationToken)
    {
        try
        {
            switch (command.Type)
            {
                case DeleteType.Cluster:
                    var cluster = await _context.ClusterContext.FindAsync(command.Id);
                    if (cluster != null)
                    {
                        _context.ClusterContext.Remove(cluster);
                    }
                    else
                    {
                        _logger.LogWarning("No cluster found with ID {Id}", command.Id);
                        return "Entity not found";
                    }
                    break;
                case DeleteType.Domain:
                    var domainInfo = await _context.DomainContext.FindAsync(command.Id);
                    if (domainInfo != null)
                    {
                        _context.DomainContext.Remove(domainInfo);
                    }
                    else
                    {
                        _logger.LogWarning("No domain found with ID {Id}", command.Id);
                        return "Entity not found";
                    }
                    break;
                case DeleteType.ConfigFile:
                    var configFile = await _context.ConfigFile.FindAsync(command.Id);
                    if (configFile != null)
                    {
                        _context.ConfigFile.Remove(configFile);
                    }
                    else
                    {
                        _logger.LogWarning("No ConfigFile found with ID {Id}", command.Id);
                        return "Entity not found";
                    }
                    break;
                case DeleteType.ConfigMap:
                    var configMap = await _context.ConfigMap.FindAsync(command.Id);
                    if (configMap != null)
                    {
                        _context.ConfigMap.Remove(configMap);
                    }
                    else
                    {
                        _logger.LogWarning("No configMap found with ID {Id}", command.Id);
                        return "Entity not found";
                    }
                    break;
                case DeleteType.AppKeyVault:
                case DeleteType.ClusterKeyVault:     
                    var appKeyVault = await _context.KeyVaultInfoContext.FindAsync(command.Id);
                    if (appKeyVault != null)
                    {
                        _context.KeyVaultInfoContext.Remove(appKeyVault);
                    }
                    else
                    {
                        _logger.LogWarning("No KeyVault found with ID {Id}", command.Id);
                        return $"Entity [{command.Id}] not found";
                    }
                    break;
                
                case DeleteType.DiskInfo:
                    var yamlClusterDiskInfo = await _context.DiskInfoContext.FindAsync(command.Id);
                    if (yamlClusterDiskInfo != null)
                    {
                        _context.DiskInfoContext.Remove(yamlClusterDiskInfo);
                    }
                    else
                    {
                        _logger.LogWarning("No DiskInfo found with ID {Id}", command.Id);
                        return $"Entity [{command.Id}] not found ";
                    } 
                    break;
            }
            await _context.SaveChangesAsync(cancellationToken);
            return "Success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while handling DeleteCommand");
            throw new ServiceException("Error occurred while handling DeleteCommand");
        }
    }
}

