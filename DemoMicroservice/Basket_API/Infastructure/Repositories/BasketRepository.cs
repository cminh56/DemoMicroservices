using Basket_API.Domain.Entities;
using Basket_API.Domain.Interfaces;
using StackExchange.Redis;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Basket_API.Infastructure.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDatabase _db;
        public BasketRepository(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<Basket?> GetByIdAsync(Guid userId)
        {
            var data = await _db.StringGetAsync(userId.ToString());
            return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<Basket>(data!);
        }

        public async Task<IEnumerable<Basket>> GetAllAsync()
        {
            // Redis không hỗ trợ duyệt toàn bộ key theo chuẩn, demo trả về empty list
            return new List<Basket>();
        }

        public async Task<Basket> AddAsync(Basket basket)
        {
            var existing = await GetByIdAsync(basket.UserId);
            if (existing != null)
            {
                foreach (var item in basket.Items)
                {
                    var existItem = existing.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
                    if (existItem != null)
                    {
                        existItem.Quantity = item.Quantity;
                    }
                    else
                    {
                        existing.Items.Add(item);
                    }
                }
                await _db.StringSetAsync(basket.UserId.ToString(), JsonSerializer.Serialize(existing));
                return existing;
            }
            else
            {
                await _db.StringSetAsync(basket.UserId.ToString(), JsonSerializer.Serialize(basket));
                return basket;
            }
        }

        public async Task<Basket> UpdateBasketItemAsync(Guid userId, BasketItem item)
        {
            var basket = await GetByIdAsync(userId) ?? new Basket { UserId = userId };
            var existing = basket.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existing != null)
            {
                existing.Quantity = item.Quantity;
            }
            else
            {
                basket.Items.Add(item);
            }
            await _db.StringSetAsync(userId.ToString(), JsonSerializer.Serialize(basket));
            return basket;
        }

        public async Task<Basket> DeleteBasketItemAsync(Guid userId, Guid productId)
        {
            var basket = await GetByIdAsync(userId);
            if (basket == null) return null;
            basket.Items = basket.Items.Where(i => i.ProductId != productId).ToList();
            await _db.StringSetAsync(userId.ToString(), JsonSerializer.Serialize(basket));
            return basket;
        }

        public async Task DeleteAsync(Guid userId)
        {
            await _db.KeyDeleteAsync(userId.ToString());
        }
    }
}