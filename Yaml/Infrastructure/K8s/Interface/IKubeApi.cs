namespace Yaml.Infrastructure.K8s;

public interface IKubeApi
{
  
    public string CreateNamespace(string ns);
}