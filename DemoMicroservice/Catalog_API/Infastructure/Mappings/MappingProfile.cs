using AutoMapper;
using Catalog_API.Domain.Entities;
using Catalog_API.Common.DTO;

namespace Catalog_API.Infastructure.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category, CategoryDTO>().ReverseMap();
        }
    }
} 