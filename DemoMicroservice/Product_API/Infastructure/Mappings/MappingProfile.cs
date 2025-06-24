using AutoMapper;
using Product_API.Common.DTO;
using Product_API.Domain.Entities;

namespace Product_API.Infastructure.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.CategoryID, opt => opt.MapFrom(src => src.CategoryID));
            CreateMap<CreateProductDTO, Product>()
                .ForMember(dest => dest.CategoryID, opt => opt.MapFrom(src => src.CategoryID));
            CreateMap<UpdateProductDTO, Product>()
                .ForMember(dest => dest.CategoryID, opt => opt.MapFrom(src => src.CategoryID));
        }
    }
} 