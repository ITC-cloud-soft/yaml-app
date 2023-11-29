using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Msi.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Yaml.Domain.AzureApi.Interface;

namespace Yaml.Domain.AzureApi;

public class AzureIdentityManager: IAzureIdentityManager
{
    private readonly IAzure _azure;
    
    public AzureIdentityManager(string clientId, string clientSecret, string tenantId, string subscriptionId)
    {
        var credentials = SdkContext.AzureCredentialsFactory
            .FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);

        _azure = Azure
            .Configure()
            .Authenticate(credentials)
            .WithSubscription(subscriptionId);
    }

    public async Task<IIdentity> CreateIdentityAsync(string identityName, string resourceGroupName)
    {
        var resourceGroup = await _azure.ResourceGroups.GetByNameAsync(resourceGroupName);

        var identity = await _azure.Identities.Define(identityName)
            .WithRegion(resourceGroup.Region)
            .WithExistingResourceGroup(resourceGroupName)
            .CreateAsync();
        return identity;
    }
}