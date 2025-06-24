using Grpc.Net.Client;
using Order_API.Protos;

namespace Order_API.Application.Services;

public interface IInventoryClientService
{
    Task<int> GetProductQuantityAsync(Guid productId);
    Task<bool> UpdateProductQuantityAsync(Guid productId, int quantity);
    Task<Dictionary<Guid, int>> GetProductQuantitiesAsync(IEnumerable<Guid> productIds);
}

public class InventoryClientService : IInventoryClientService
{
    private readonly InventoryService.InventoryServiceClient _client;
    private readonly ILogger<InventoryClientService> _logger;

    public InventoryClientService(ILogger<InventoryClientService> logger, IConfiguration configuration)
    {
        _logger = logger;
        
        var inventoryServiceUrl = configuration["InventoryService:Url"];
        var channel = GrpcChannel.ForAddress(inventoryServiceUrl);
        _client = new InventoryService.InventoryServiceClient(channel);
    }

    public async Task<int> GetProductQuantityAsync(Guid productId)
    {
        try
        {
            var request = new GetProductQuantityRequest
            {
                ProductId = productId.ToString()
            };

            var response = await _client.GetProductQuantityAsync(request);
            
            if (!response.Success)
            {
                _logger.LogWarning("Failed to get product quantity for {ProductId}: {Message}", productId, response.Message);
                return 0;
            }

            _logger.LogInformation("[InventoryClientService] ProductId: {ProductId}, Quantity: {Quantity}", productId, response.Quantity);

            return response.Quantity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product quantity for {ProductId}", productId);
            return 0;
        }
    }

    public async Task<bool> UpdateProductQuantityAsync(Guid productId, int quantity)
    {
        try
        {
            var request = new UpdateProductQuantityRequest
            {
                ProductId = productId.ToString(),
                Quantity = quantity
            };

            var response = await _client.UpdateProductQuantityAsync(request);
            
            if (!response.Success)
            {
                _logger.LogWarning("Failed to update product quantity for {ProductId}: {Message}", productId, response.Message);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product quantity for {ProductId}", productId);
            return false;
        }
    }

    public async Task<Dictionary<Guid, int>> GetProductQuantitiesAsync(IEnumerable<Guid> productIds)
    {
        try
        {
            var request = new GetProductQuantitiesRequest();
            request.ProductIds.AddRange(productIds.Select(id => id.ToString()));

            var response = await _client.GetProductQuantitiesAsync(request);
            
            if (!response.Success)
            {
                _logger.LogWarning("Failed to get product quantities: {Message}", response.Message);
                return new Dictionary<Guid, int>();
            }

            var result = new Dictionary<Guid, int>();
            foreach (var item in response.Items)
            {
                if (Guid.TryParse(item.ProductId, out var productId))
                {
                    result[productId] = item.Quantity;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product quantities");
            return new Dictionary<Guid, int>();
        }
    }
} 