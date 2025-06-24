using Grpc.Core;
using Inventory_API.Protos;
using Inventory_API.Application.Services;

namespace Inventory_API.Application.Services;

public class InventoryGrpcService : Inventory_API.Protos.InventoryService.InventoryServiceBase
{
    private readonly InventoryService _inventoryService;

    public InventoryGrpcService(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public override async Task<GetProductQuantityResponse> GetProductQuantity(GetProductQuantityRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ProductId, out var productId))
            {
                return new GetProductQuantityResponse
                {
                    ProductId = request.ProductId,
                    Quantity = 0,
                    Success = false,
                    Message = "Invalid product ID format"
                };
            }

            var quantityResponse = await _inventoryService.GetProductQuantityAsync(productId);
            
            if (quantityResponse == null)
            {
                return new GetProductQuantityResponse
                {
                    ProductId = request.ProductId,
                    Quantity = 0,
                    Success = false,
                    Message = "Product not found in inventory"
                };
            }

            return new GetProductQuantityResponse
            {
                ProductId = request.ProductId,
                Quantity = quantityResponse.AvailableQuantity,
                Success = true,
                Message = "Success"
            };
        }
        catch (Exception ex)
        {
            return new GetProductQuantityResponse
            {
                ProductId = request.ProductId,
                Quantity = 0,
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public override async Task<UpdateProductQuantityResponse> UpdateProductQuantity(UpdateProductQuantityRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ProductId, out var productId))
            {
                return new UpdateProductQuantityResponse
                {
                    ProductId = request.ProductId,
                    NewQuantity = 0,
                    Success = false,
                    Message = "Invalid product ID format"
                };
            }

            if (request.Quantity < 0)
            {
                return new UpdateProductQuantityResponse
                {
                    ProductId = request.ProductId,
                    NewQuantity = 0,
                    Success = false,
                    Message = "Quantity cannot be negative"
                };
            }

            var success = await _inventoryService.UpdateProductQuantityAsync(productId, request.Quantity);
            
            if (!success)
            {
                return new UpdateProductQuantityResponse
                {
                    ProductId = request.ProductId,
                    NewQuantity = 0,
                    Success = false,
                    Message = "Product not found in inventory"
                };
            }

            return new UpdateProductQuantityResponse
            {
                ProductId = request.ProductId,
                NewQuantity = request.Quantity,
                Success = true,
                Message = "Quantity updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new UpdateProductQuantityResponse
            {
                ProductId = request.ProductId,
                NewQuantity = 0,
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public override async Task<GetProductQuantitiesResponse> GetProductQuantities(GetProductQuantitiesRequest request, ServerCallContext context)
    {
        try
        {
            var productIds = new List<Guid>();
            
            foreach (var productIdStr in request.ProductIds)
            {
                if (Guid.TryParse(productIdStr, out var productId))
                {
                    productIds.Add(productId);
                }
            }

            if (!productIds.Any())
            {
                return new GetProductQuantitiesResponse
                {
                    Success = false,
                    Message = "No valid product IDs provided"
                };
            }

            var quantitiesResponse = await _inventoryService.GetProductQuantitiesAsync(productIds);
            
            var response = new GetProductQuantitiesResponse
            {
                Success = true,
                Message = "Success"
            };

            foreach (var item in quantitiesResponse.Items)
            {
                response.Items.Add(new ProductQuantity
                {
                    ProductId = item.ProductId.ToString(),
                    Quantity = item.AvailableQuantity
                });
            }

            return response;
        }
        catch (Exception ex)
        {
            return new GetProductQuantitiesResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }
} 