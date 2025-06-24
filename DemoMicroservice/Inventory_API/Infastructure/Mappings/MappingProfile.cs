using AutoMapper;
using Inventory_API.Domain.Entities;
using Inventory_API.Common.DTO;

namespace Inventory_API.Infastructure.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Inventory, InventoryDTO>();
        CreateMap<CreateInventoryDTO, Inventory>();
        CreateMap<UpdateInventoryDTO, Inventory>();
        
        CreateMap<Inventory, QuantityResponseDTO>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.AvailableQuantity));
    }
} 