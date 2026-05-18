using AutoMapper;
using Wms.Application.Warehouse.Dtos;
using WarehouseEntity = Wms.Domain.Entities.Warehouse.Warehouse;

namespace Wms.Application.Warehouse.Mappings;

public sealed class WarehouseMappingProfile : Profile
{
    public WarehouseMappingProfile()
    {
        CreateMap<WarehouseEntity, WarehouseDto>();
        CreateMap<CreateWarehouseDto, WarehouseEntity>();
        CreateMap<UpdateWarehouseDto, WarehouseEntity>();
        CreateMap<SyncWarehouseDto, WarehouseEntity>();
    }
}
