using System.Collections.Generic;

namespace Yaml.Infrastructure.Dto;

public class KeyVaultRender
{
    public string KeyVaultName { get; set; }
    public List<string> ConfigKeyList { get; set; }
    public string AppName { get; set; }
    public string ManagedId { get; set; }
    public string TenantId { get; set; }
}

