using AutoMapper;
using Wms.Application.Stock.Dtos;
using Wms.Domain.Entities.Stock;
using StockEntity = Wms.Domain.Entities.Stock.Stock;

namespace Wms.Application.Stock.Mappings;

public sealed class StockMappingProfile : Profile
{
    public StockMappingProfile()
    {
        CreateMap<StockEntity, StockDto>();
        CreateMap<CreateStockDto, StockEntity>();
        CreateMap<UpdateStockDto, StockEntity>();
        CreateMap<SyncStockDto, StockEntity>();

        CreateMap<StockDetail, StockDetailDto>()
            .ForMember(d => d.ErpStockCode, o => o.MapFrom(s => s.Stock.ErpStockCode))
            .ForMember(d => d.StockName, o => o.MapFrom(s => s.Stock.StockName));

        CreateMap<StockImage, StockImageDto>()
            .ForMember(d => d.ErpStockCode, o => o.MapFrom(s => s.Stock.ErpStockCode))
            .ForMember(d => d.StockName, o => o.MapFrom(s => s.Stock.StockName));
    }
}
