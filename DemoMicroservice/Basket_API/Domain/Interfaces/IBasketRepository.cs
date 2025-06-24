using Basket_API.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Basket_API.Domain.Interfaces
{
    public interface IBasketRepository
    {
        Task<Basket?> GetByIdAsync(Guid userId);
        Task<IEnumerable<Basket>> GetAllAsync();
        Task<Basket> AddAsync(Basket basket);
        Task<Basket> UpdateBasketItemAsync(Guid userId, BasketItem item);
        Task<Basket> DeleteBasketItemAsync(Guid userId, Guid productId);
        Task DeleteAsync(Guid userId);
    }
} 