using AutoMapper;
using Wms.Application.AccessControl.Dtos;
using Wms.Domain.Entities.AccessControl;

namespace Wms.Application.AccessControl.Mappings;

public sealed class AccessControlMappingProfile : Profile
{
    public AccessControlMappingProfile()
    {
        CreateMap<PermissionDefinition, PermissionDefinitionDto>();
        CreateMap<WmsScopePolicy, WmsScopePolicyDto>();
    }
}
