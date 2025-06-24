using AutoMapper;
using Order_API.Domain.Entities;
using Order_API.Common.DTO;

namespace Order_API.Infastructure.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Order, OrderDTO>().ReverseMap();
            CreateMap<OrderDetail, OrderDetailDTO>().ReverseMap();
            CreateMap<AddOrderDTO, Order>()
                .ForMember(dest => dest.OrderDate, opt => opt.Ignore());
            CreateMap<UpdateOrderDTO, Order>();
            CreateMap<AddOrderDetailDTO, OrderDetail>();
            CreateMap<UpdateOrderDetailDTO, OrderDetail>();
        }
    }
} 