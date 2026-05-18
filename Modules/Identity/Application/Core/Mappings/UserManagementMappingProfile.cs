using AutoMapper;
using Wms.Application.Identity.Dtos;
using Wms.Domain.Entities.Identity;

namespace Wms.Application.Identity.Mappings;

public sealed class UserManagementMappingProfile : Profile
{
    public UserManagementMappingProfile()
    {
        CreateMap<UserAuthority, UserAuthorityDto>();
        CreateMap<CreateUserAuthorityDto, UserAuthority>();
        CreateMap<UpdateUserAuthorityDto, UserAuthority>();

        CreateMap<UserDetail, UserDetailDto>();
        CreateMap<CreateUserDetailDto, UserDetail>();
        CreateMap<UpdateUserDetailDto, UserDetail>();

        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();
    }
}
