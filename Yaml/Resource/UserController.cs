using System.Text;
using FluentValidation;
using k8s;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Yaml.Application.Command;
using Yaml.Infrastructure.Exception;

namespace Yaml.Resource;

public class UserController : ApiControllerBase
{
    private readonly IValidator<UserLoginCommand> _validator;
    private readonly IStringLocalizer<UserController> _localizer;
    private readonly IMemoryCache _memoryCache;
    private readonly IStringLocalizer<SharedResources> _sharedlocalizer;
    private readonly string _currentDirectory = Directory.GetCurrentDirectory();
    
    public UserController(
        IValidator<UserLoginCommand> validator, 
        IStringLocalizer<UserController> localizer,
        IStringLocalizer<SharedResources> sharedlocalizer,
        IMemoryCache memory
        )
    {
        _validator = validator;
        _localizer = localizer;
        _memoryCache = memory;
        _sharedlocalizer = sharedlocalizer;
    }
    private Kubernetes GetKubeClient()
    {
        var client = _memoryCache.Get<Kubernetes>(1);
        if (client != null)
        {
            return client;
        }


        try
        {
            var configFile = Path.Combine(_currentDirectory, "uploads/fb5a19b1-6fa8-4c8d-9313-82c40a7a7e6e_config");
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(@configFile);
            client = new Kubernetes(config);

            _memoryCache.Set(1, client);
        }
        catch (Exception ex)
        {
            throw new ServiceException("Error creating Kubernetes client.", ex);
        }

        return client ?? throw new ServiceException("Client creation failed unexpectedly.");
    }

    

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginCommand command)
    {
        var response = await Mediator.Send(command);
        return StatusCode((int)response.Status, response);
    }    
    
    // [HttpPost("azureLogin")]
    // public async Task<IActionResult> Login()
    // {
    //     try
    //     {
    //         // az loign
    //         var clientId = "";
    //         var clientSecret = "";
    //         var tenantId = "";
    //         var resourceGroupName = "";
    //         var clusterName = "";
    //         // Azure 认证
    //         var credentials = SdkContext.AzureCredentialsFactory
    //             .FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
    //
    //         var azure = Azure
    //             .Configure()
    //             .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
    //             .Authenticate(credentials)
    //             .WithDefaultSubscription();
    //
    //         // 获取 AKS 集群信息
    //         var cluster = azure.KubernetesClusters.GetByResourceGroup(resourceGroupName, clusterName);
    //
    //         // 将 kube config byte[] 转换为字符串
    //         var kubeConfigEncoded = cluster.UserKubeConfigContent;
    //         var kubeConfig = Encoding.UTF8.GetString(kubeConfigEncoded);
    //
    //         // 将字符串保存到临时文件
    //         var tempKubeConfigPath = Path.GetTempFileName();
    //         await System.IO.File.WriteAllTextAsync(tempKubeConfigPath, kubeConfig);
    //
    //         // 使用文件创建 Kubernetes 客户端配置
    //         var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(tempKubeConfigPath);
    //         Kubernetes client = new Kubernetes(config);
    //
    //         // Action
    //         var listNamespaceAsync = await client.ListNamespaceAsync();
    //         foreach (var names in listNamespaceAsync)
    //         {
    //             Console.WriteLine(names.Metadata.Name);
    //         }
    //
    //         return Ok();
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //         throw;
    //     }
    // }
    
    [HttpPost("configLogin")]
    public async Task<IActionResult> LoginByKubeConfig()
    {
        try
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(
                "/Users/yanzou/RiderProjects/YamlController/Yaml/uploads/fb5a19b1-6fa8-4c8d-9313-82c40a7a7e6e_config"
            );
       
            var listNamespaceAsync = await GetKubeClient().ListNamespaceAsync();
            foreach (var names in listNamespaceAsync)
            {
                Console.WriteLine(names.Metadata.Name);
            }

            return Ok(listNamespaceAsync);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}