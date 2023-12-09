using System.Text;
using FluentValidation;
using k8s;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Localization;
using Yaml.Application.Command;

namespace Yaml.Resource;

public class UserController : ApiControllerBase
{
    private readonly IValidator<UserLoginCommand> _validator;
    private readonly IStringLocalizer<UserController> _localizer;
    private readonly IStringLocalizer<SharedResources> _sharedlocalizer;

    public UserController(
        IValidator<UserLoginCommand> validator, 
        IStringLocalizer<UserController> localizer,
        IStringLocalizer<SharedResources> sharedlocalizer)
    {
        _validator = validator;
        _localizer = localizer;
        _sharedlocalizer = sharedlocalizer;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginCommand command)
    {
        var response = await Mediator.Send(command);
        return StatusCode((int)response.Status, response);
    }    
    
    [HttpPost("azureLogin")]
    public async Task<IActionResult> Login()
    {
        try
        {
            // az loign
            var clientId = "";
            var clientSecret = "";
            var tenantId = "";
            var resourceGroupName = "";
            var clusterName = "";
            // Azure 认证
            var credentials = SdkContext.AzureCredentialsFactory
                .FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);

            var azure = Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithDefaultSubscription();

            // 获取 AKS 集群信息
            var cluster = azure.KubernetesClusters.GetByResourceGroup(resourceGroupName, clusterName);

            // 将 kube config byte[] 转换为字符串
            var kubeConfigEncoded = cluster.UserKubeConfigContent;
            var kubeConfig = Encoding.UTF8.GetString(kubeConfigEncoded);

            // 将字符串保存到临时文件
            var tempKubeConfigPath = Path.GetTempFileName();
            await System.IO.File.WriteAllTextAsync(tempKubeConfigPath, kubeConfig);

            // 使用文件创建 Kubernetes 客户端配置
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(tempKubeConfigPath);
            Kubernetes client = new Kubernetes(config);

            // Action
            var listNamespaceAsync = await client.ListNamespaceAsync();
            foreach (var names in listNamespaceAsync)
            {
                Console.WriteLine(names.Metadata.Name);
            }

            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpPost("configLogin")]
    public async Task<IActionResult> LoginByKubeConfig()
    {
        try
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(
                @"/Users/yanzou/Downloads/config", // 配置文件的路径
                "k8stest"
            );
            
            Kubernetes client = new Kubernetes(config);
            var listNamespaceAsync = await client.ListNamespaceAsync();
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