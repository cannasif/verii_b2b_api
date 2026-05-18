using AutoMapper;
using Wms.Application.YapKod.Dtos;
using YapKodEntity = Wms.Domain.Entities.YapKod.YapKod;

namespace Wms.Application.YapKod.Mappings;

public sealed class YapKodMappingProfile : Profile
{
    public YapKodMappingProfile()
    {
        CreateMap<YapKodEntity, YapKodDto>()
            .ForMember(dest => dest.YapKod, opt => opt.MapFrom(src => src.YapKodCode));

        CreateMap<CreateYapKodDto, YapKodEntity>()
            .ForMember(dest => dest.YapKodCode, opt => opt.MapFrom(src => src.YapKod));

        CreateMap<UpdateYapKodDto, YapKodEntity>()
            .ForMember(dest => dest.YapKodCode, opt => opt.MapFrom(src => src.YapKod));

        CreateMap<SyncYapKodDto, YapKodEntity>()
            .ForMember(dest => dest.YapKodCode, opt => opt.MapFrom(src => src.YapKod));
    }
}
