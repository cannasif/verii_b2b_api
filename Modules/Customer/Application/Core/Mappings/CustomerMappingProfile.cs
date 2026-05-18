using AutoMapper;
using Wms.Application.Customer.Dtos;
using CustomerEntity = Wms.Domain.Entities.Customer.Customer;

namespace Wms.Application.Customer.Mappings;

public sealed class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<CustomerEntity, CustomerDto>();
        CreateMap<CreateCustomerDto, CustomerEntity>();
        CreateMap<UpdateCustomerDto, CustomerEntity>();
        CreateMap<SyncCustomerDto, CustomerEntity>();
    }
}
