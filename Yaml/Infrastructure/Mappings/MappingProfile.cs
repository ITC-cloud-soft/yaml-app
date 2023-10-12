using System.Reflection;
using AutoMapper;
using Yaml.Domain.Entity;
using Yaml.Infrastructure.Dto;
namespace Yaml.Infrastructure.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);
        
        var mappingMethodName = nameof(IMapFrom<object>.Mapping);
        
        bool HasInterface(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == mapFromType;
        
        var types = assembly.GetExportedTypes().Where(t => t.GetInterfaces().Any(HasInterface)).ToList();
        
        var argumentTypes = new Type[] { typeof(Profile) };
        
        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
        
            var methodInfo = type.GetMethod(mappingMethodName);
        
            if (methodInfo != null)
            {
                methodInfo.Invoke(instance, new object[] { this });
            }
            else
            {
                var interfaces = type.GetInterfaces().Where(HasInterface).ToList();
        
                if (interfaces.Count > 0)
                {
                    foreach (var @interface in interfaces)
                    {
                        var interfaceMethodInfo = @interface.GetMethod(mappingMethodName, argumentTypes);
        
                        interfaceMethodInfo?.Invoke(instance, new object[] { this });
                    }
                }
            }
        }

        // DTO => Entity
        CreateMap<YamlAppInfoDto, YamlAppInfo>()
            .ForMember(dest => dest.Tenantid, opt => opt.MapFrom(src => src.KeyVault.TenantId))
            .ForMember(dest => dest.KeyVaultName, opt => opt.MapFrom(src => src.KeyVault.KeyVaultName))
            .ForMember(dest => dest.ManagedId, opt => opt.MapFrom(src => src.KeyVault.ManagedId));
        
        CreateMap<YamlClusterInfoDto, YamlClusterInfo>();
        CreateMap<DomainDto, YamlClusterDomainInfo>();
        CreateMap<ConfigMapDto, YamlClusterConfigMapInfo>();
        CreateMap<ConfigFileDto, YamlClusterConfigFileInfo>();

        // Entity => DOT
        CreateMap<YamlAppInfo, YamlAppInfoDto>();
        CreateMap<YamlClusterInfo, YamlClusterInfoDto>();
        CreateMap<YamlClusterDomainInfo, DomainDto>();
        CreateMap<YamlClusterConfigMapInfo, ConfigMapDto>();
        CreateMap<YamlClusterConfigFileInfo, ConfigFileDto>();
        CreateMap<YamlKeyVaultInfo, KeyVaultDto>();

    }
}
