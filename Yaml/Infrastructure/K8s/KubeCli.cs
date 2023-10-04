using k8s;
using k8s.Models;
using RazorLight;
using Yaml.Infrastructure.Dto;

namespace Yaml.Infrastructure.K8s;

public class KubeCli : IKubeApi
{
    
    private readonly  IRazorLightEngine _engine;
    private readonly  Kubernetes _client;

    public KubeCli(IRazorLightEngine engine, Kubernetes client)
    {
        _engine = engine;
        _client = client;
    }

    public Task<V1Namespace> CreateNamespace(string namespaceName, CancellationToken cancellationToken)
    {
 
        // if not exist
        var namespaceAsync = _client.CreateNamespaceAsync(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = namespaceName }
        }, cancellationToken: cancellationToken);
        return namespaceAsync;
    }
    
    public async Task<string> CreateService(YamlAppInfoDto dto, CancellationToken cancellationToken)
    {
        var pods = await _client.ListPodForAllNamespacesAsync(cancellationToken: cancellationToken);
        foreach (var pod in pods.Items)
        {
            Console.WriteLine($"Namespace: {pod.Metadata.NamespaceProperty}, Pod Name: {pod.Metadata.Name}");
        }
        return "123;";
    }
}