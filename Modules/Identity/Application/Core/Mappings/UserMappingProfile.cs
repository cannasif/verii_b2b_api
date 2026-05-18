using AutoMapper;
using Wms.Application.Identity.Dtos;
using Wms.Domain.Entities.Identity;

namespace Wms.Application.Identity.Mappings;

public sealed class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.RoleNavigation != null ? src.RoleNavigation.Title : "User"))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(_ => 1L))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));
    }
}
