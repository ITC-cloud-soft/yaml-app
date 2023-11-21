namespace Yaml.Domain;

public static class KubeConstants
{
    public const string ConfigMapTemplate = "YamlFile/ConfigMap.cshtml";
    public const string PersistentVolumeClaimTemplate = "YamlFile/PersistentVolumeClaim.cshtml";
    public const string DeploymentTemplate = "YamlFile/Deployment.cshtml";
    public const string ServiceTemplate = "YamlFile/Service.cshtml";
    public const string SecretTemplate = "YamlFile/Keyvault.cshtml";
    public const string OutPutFile = "YamlFile/App.yaml";
    public const string IngressFile = "YamlFile/Ingress.cshtml";

    public const string NamespaceSuffix = "-ns";
    public const string PvcSubSuffix = "-pvc";
    public const string ConfigMapSuffix = "-configmap";
    public const string ServiceSuffix = "-svc";
    public const string DeploymentSuffix = "-deployment";
    public const string SecretSuffix = "-secret";
    public const string IngressSuffix = "-ingress";
}