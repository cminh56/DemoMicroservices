using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Inventory_API.Application.Services;
using Inventory_API.Common.DTO;
using Inventory_API.Common.Helpers;

namespace Inventory_API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class InventoryController : ControllerBase
{
    private readonly InventoryService _inventoryService;

    public InventoryController(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<InventoryDTO>>>> GetAll()
    {
        try
        {
            var inventories = await _inventoryService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<InventoryDTO>>(200, "Inventories retrieved successfully", inventories));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<IEnumerable<InventoryDTO>>(500, $"Error: {ex.Message}", null));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<InventoryDTO>>> GetById(Guid id)
    {
        try
        {
            var inventory = await _inventoryService.GetByIdAsync(id);
            if (inventory == null)
            {
                return NotFound(new ApiResponse<InventoryDTO>(404, "Inventory not found", null));
            }

            return Ok(new ApiResponse<InventoryDTO>(200, "Inventory retrieved successfully", inventory));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<InventoryDTO>(500, $"Error: {ex.Message}", null));
        }
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<InventoryDTO>>> GetByProductId(Guid productId)
    {
        try
        {
            var inventory = await _inventoryService.GetByProductIdAsync(productId);
            if (inventory == null)
            {
                return NotFound(new ApiResponse<InventoryDTO>(404, "Inventory not found for this product", null));
            }

            return Ok(new ApiResponse<InventoryDTO>(200, "Inventory retrieved successfully", inventory));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<InventoryDTO>(500, $"Error: {ex.Message}", null));
        }
    }

    [HttpGet("product/{productId:guid}/quantity")]
    public async Task<ActionResult<ApiResponse<QuantityResponseDTO>>> GetProductQuantity(Guid productId)
    {
        try
        {
            var quantity = await _inventoryService.GetProductQuantityAsync(productId);
            if (quantity == null)
            {
                return NotFound(new ApiResponse<QuantityResponseDTO>(404, "Product not found in inventory", null));
            }

            return Ok(new ApiResponse<QuantityResponseDTO>(200, "Product quantity retrieved successfully", quantity));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<QuantityResponseDTO>(500, $"Error: {ex.Message}", null));
        }
    }

    [HttpPost("quantities")]
    public async Task<ActionResult<ApiResponse<QuantitiesResponseDTO>>> GetProductQuantities([FromBody] GetQuantitiesDTO request)
    {
        try
        {
            if (request.ProductIds == null || !request.ProductIds.Any())
            {
                return BadRequest(new ApiResponse<QuantitiesResponseDTO>(400, "Product IDs are required", null));
            }

            var quantities = await _inventoryService.GetProductQuantitiesAsync(request.ProductIds);
            return Ok(new ApiResponse<QuantitiesResponseDTO>(200, "Product quantities retrieved successfully", quantities));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<QuantitiesResponseDTO>(500, $"Error: {ex.Message}", null));
        }
    }

    [HttpPost]
    //[Authorize]
    public async Task<ActionResult<ApiResponse<InventoryDTO>>> Create([FromBody] CreateInventoryDTO createDto)
    {
        try
        {
            if (createDto.Quantity < 0)
            {
                return BadRequest(new ApiResponse<InventoryDTO>(400, "Quantity cannot be negative", null));
            }

            var inventory = await _inventoryService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = inventory.Id }, new ApiResponse<InventoryDTO>(200, "Inventory created successfully", inventory));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<InventoryDTO>(500, $"Error: {ex.Message}", null));
        }
    }

    [HttpPut("{id:guid}")]
    //[Authorize]
    public async Task<ActionResult<ApiResponse<InventoryDTO>>> Update(Guid id, [FromBody] UpdateInventoryDTO updateDto)
    {
        try
        {
            if (updateDto.Quantity < 0)
            {
                return BadRequest(new ApiResponse<InventoryDTO>(400, "Quantity cannot be negative", null));
            }

            var inventory = await _inventoryService.UpdateAsync(id, updateDto);
            if (inventory == null)
            {
                return NotFound(new ApiResponse<InventoryDTO>(404, "Inventory not found", null));
            }

            return Ok(new ApiResponse<InventoryDTO>(200, "Inventory updated successfully", inventory));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<InventoryDTO>(500, $"Error: {ex.Message}", null));
        }
    }

    [HttpPut("product/{productId:guid}/quantity")]
    //[Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateProductQuantity(Guid productId, [FromBody] UpdateQuantityDTO updateDto)
    {
        try
        {
            if (updateDto.Quantity < 0)
            {
                return BadRequest(new ApiResponse<bool>(400, "Quantity cannot be negative", false));
            }

            var success = await _inventoryService.UpdateProductQuantityAsync(productId, updateDto.Quantity);
            if (!success)
            {
                return NotFound(new ApiResponse<bool>(404, "Product not found in inventory", false));
            }

            return Ok(new ApiResponse<bool>(200, "Product quantity updated successfully", true));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>(500, $"Error: {ex.Message}", false));
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var success = await _inventoryService.DeleteAsync(id);
            if (!success)
            {
                return NotFound(new ApiResponse<bool>(404, "Inventory not found", false));
            }

            return Ok(new ApiResponse<bool>(200, "Inventory deleted successfully", true));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<bool>(500, $"Error: {ex.Message}", false));
        }
    }
} 