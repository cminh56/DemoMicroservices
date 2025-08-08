using Grpc.Net.Client;
using Inventory_API.Protos;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace Product_API.Application.Services
{
    public class InventoryGrpcClientService
    {
        private readonly InventoryService.InventoryServiceClient _client;
        private readonly ILogger<InventoryGrpcClientService> _logger;
        private readonly ConcurrentDictionary<Guid, int> _quantityCache = new();
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
        private DateTime _lastCacheRefresh = DateTime.MinValue;

        public InventoryGrpcClientService(InventoryService.InventoryServiceClient client, ILogger<InventoryGrpcClientService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation("InventoryGrpcClientService initialized");
        }

        public async Task<int> GetProductQuantityAsync(Guid productId)
        {
            try
            {
                // Check cache first
                if (_quantityCache.TryGetValue(productId, out int cachedQuantity) && 
                    DateTime.UtcNow - _lastCacheRefresh < _cacheDuration)
                {
                    return cachedQuantity;
                }

                var request = new GetProductQuantityRequest
                {
                    ProductId = productId.ToString()
                };

                var response = await _client.GetProductQuantityAsync(request);

                if (response.Success)
                {
                    // Update cache
                    _quantityCache.AddOrUpdate(productId, response.Quantity, (_, _) => response.Quantity);
                    return response.Quantity;
                }
                else
                {
                    _logger.LogWarning("Failed to get product quantity: {Message}", response.Message);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling inventory service for product {ProductId}", productId);
                return 0; // Return 0 instead of failing
            }
        }

        public async Task<Dictionary<Guid, int>> GetProductQuantitiesAsync(IEnumerable<Guid> productIds)
        {
            try
            {
                var result = new Dictionary<Guid, int>();
                
                // If no product IDs, return empty dictionary
                if (!productIds.Any())
                {
                    return result;
                }
                
                var productIdsToFetch = new List<Guid>();
                
                // Check which products we need to fetch vs which are in cache
                foreach (var productId in productIds)
                {
                    if (_quantityCache.TryGetValue(productId, out int cachedQuantity) && 
                        DateTime.UtcNow - _lastCacheRefresh < _cacheDuration)
                    {
                        result[productId] = cachedQuantity;
                    }
                    else
                    {
                        productIdsToFetch.Add(productId);
                    }
                }

                // If all products were in cache, return early
                if (!productIdsToFetch.Any())
                {
                    return result;
                }

                var request = new GetProductQuantitiesRequest();
                foreach (var productId in productIdsToFetch)
                {
                    request.ProductIds.Add(productId.ToString());
                }

                var response = await _client.GetProductQuantitiesAsync(request);

                if (response.Success)
                {
                    _lastCacheRefresh = DateTime.UtcNow;
                    
                    foreach (var item in response.Items)
                    {
                        if (Guid.TryParse(item.ProductId, out var id))
                        {
                            result[id] = item.Quantity;
                            // Update cache
                            _quantityCache.AddOrUpdate(id, item.Quantity, (_, _) => item.Quantity);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to get product quantities: {Message}", response.Message);
                }

                // For any products that weren't returned, set quantity to 0
                foreach (var productId in productIdsToFetch)
                {
                    if (!result.ContainsKey(productId))
                    {
                        result[productId] = 0;
                        _quantityCache.AddOrUpdate(productId, 0, (_, _) => 0);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling inventory service for multiple products");
                // Return dictionary with all products having 0 quantity instead of failing
                return productIds.ToDictionary(id => id, _ => 0);
            }
        }

        public void InvalidateCache()
        {
            _quantityCache.Clear();
            _lastCacheRefresh = DateTime.MinValue;
        }
    }
} 