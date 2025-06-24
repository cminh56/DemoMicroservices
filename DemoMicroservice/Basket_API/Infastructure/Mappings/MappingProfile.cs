using AutoMapper;
using Basket_API.Common.DTO;
using Basket_API.Domain.Entities;

namespace Basket_API.Infastructure.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Basket, BasketDTO>().ReverseMap();
            CreateMap<BasketItem, BasketItemDTO>().ReverseMap();
        }
    }
}