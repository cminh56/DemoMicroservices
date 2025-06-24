using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Basket_API.Domain.Entities;
using Basket_API.Domain.Interfaces;
using Basket_API.Common.Constants;

namespace Basket_API.Application.Services
{
    public class BasketService
    {
        private readonly IBasketRepository _basketRepository;
        public BasketService(IBasketRepository basketRepository)
        {
            _basketRepository = basketRepository;
        }

        public Task<IEnumerable<Basket>> GetAllAsync()
        {
            return _basketRepository.GetAllAsync();
        }

        public async Task<Basket?> GetByIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException(AppConstants.Validation.RequiredUserId);
            return await _basketRepository.GetByIdAsync(userId);
        }

        public async Task<Basket> AddAsync(Basket basket)
        {
            ValidateBasket(basket);
            return await _basketRepository.AddAsync(basket);
        }

        public async Task<Basket> UpdateBasketItemAsync(Guid userId, BasketItem item)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException(AppConstants.Validation.RequiredUserId);
            if (item.ProductId == Guid.Empty)
                throw new ArgumentException(AppConstants.Validation.RequiredProductId);
            if (item.Quantity <= 0)
                throw new ArgumentException(AppConstants.Validation.InvalidQuantity);
            return await _basketRepository.UpdateBasketItemAsync(userId, item);
        }

        public async Task<Basket> DeleteBasketItemAsync(Guid userId, Guid productId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException(AppConstants.Validation.RequiredUserId);
            if (productId == Guid.Empty)
                throw new ArgumentException(AppConstants.Validation.RequiredProductId);
            return await _basketRepository.DeleteBasketItemAsync(userId, productId);
        }

        public async Task DeleteAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException(AppConstants.Validation.RequiredUserId);
            await _basketRepository.DeleteAsync(userId);
        }

        private void ValidateBasket(Basket basket)
        {
            if (basket.UserId == Guid.Empty)
                throw new ArgumentException(AppConstants.Validation.RequiredUserId);
            if (basket.Items.Any(i => i.ProductId == Guid.Empty))
                throw new ArgumentException(AppConstants.Validation.RequiredProductId);
            if (basket.Items.Any(i => i.Quantity <= 0))
                throw new ArgumentException(AppConstants.Validation.InvalidQuantity);
        }
    }
} 