using k8s.Models;

namespace Yaml.Infrastructure.CoustomService;

public class Unstructured {

    public string? ApiVersion {get; set;}   
    public V1ObjectMeta? Metadata {get; set;}   
    public Dictionary<string, object>? Spec {get; set;}   
    public string? Kind {get; set;}   
        
}