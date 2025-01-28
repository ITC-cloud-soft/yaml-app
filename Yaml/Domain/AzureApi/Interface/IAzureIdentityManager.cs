using Microsoft.Azure.Management.Msi.Fluent;

namespace Yaml.Domain.AzureApi.Interface;

public interface IAzureIdentityManager
{
    Task<IIdentity> CreateIdentityAsync(string identityName, string resourceGroupName);
}