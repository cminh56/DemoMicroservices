using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Product_API.Domain.Entities;

namespace Product_API.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetFilteredAsync(Guid? categoryId, string? searchTerm, decimal? minPrice, decimal? maxPrice);
        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Guid id);
    }
}