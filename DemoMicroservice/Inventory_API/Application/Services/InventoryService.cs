using AutoMapper;
using Inventory_API.Domain.Entities;
using Inventory_API.Domain.Interfaces;
using Inventory_API.Common.DTO;

namespace Inventory_API.Application.Services;

public class InventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IMapper _mapper;

    public InventoryService(IInventoryRepository inventoryRepository, IMapper mapper)
    {
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<InventoryDTO>> GetAllAsync()
    {
        var inventories = await _inventoryRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<InventoryDTO>>(inventories);
    }

    public async Task<InventoryDTO?> GetByIdAsync(Guid id)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        return _mapper.Map<InventoryDTO?>(inventory);
    }

    public async Task<InventoryDTO?> GetByProductIdAsync(Guid productId)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        return _mapper.Map<InventoryDTO?>(inventory);
    }

    public async Task<QuantityResponseDTO?> GetProductQuantityAsync(Guid productId)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        return _mapper.Map<QuantityResponseDTO?>(inventory);
    }

    public async Task<QuantitiesResponseDTO> GetProductQuantitiesAsync(IEnumerable<Guid> productIds)
    {
        var inventories = await _inventoryRepository.GetByProductIdsAsync(productIds);
        var items = _mapper.Map<IEnumerable<QuantityResponseDTO>>(inventories);
        
        return new QuantitiesResponseDTO
        {
            Items = items.ToList()
        };
    }

    public async Task<InventoryDTO> CreateAsync(CreateInventoryDTO createDto)
    {
        var inventory = _mapper.Map<Inventory>(createDto);
        inventory.Id = Guid.NewGuid();
        
        var createdInventory = await _inventoryRepository.CreateAsync(inventory);
        return _mapper.Map<InventoryDTO>(createdInventory);
    }

    public async Task<InventoryDTO?> UpdateAsync(Guid id, UpdateInventoryDTO updateDto)
    {
        var existingInventory = await _inventoryRepository.GetByIdAsync(id);
        if (existingInventory == null)
            return null;

        _mapper.Map(updateDto, existingInventory);
        var updatedInventory = await _inventoryRepository.UpdateAsync(existingInventory);
        return _mapper.Map<InventoryDTO>(updatedInventory);
    }

    public async Task<bool> UpdateProductQuantityAsync(Guid productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null)
            return false;

        inventory.Quantity = quantity;
        await _inventoryRepository.UpdateAsync(inventory);
        return true;
    }

    public async Task<bool> ReserveQuantityAsync(Guid productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null || inventory.AvailableQuantity < quantity)
            return false;

        inventory.ReservedQuantity += quantity;
        await _inventoryRepository.UpdateAsync(inventory);
        return true;
    }

    public async Task<bool> ReleaseReservedQuantityAsync(Guid productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null || inventory.ReservedQuantity < quantity)
            return false;

        inventory.ReservedQuantity -= quantity;
        await _inventoryRepository.UpdateAsync(inventory);
        return true;
    }

    public async Task<bool> ConsumeQuantityAsync(Guid productId, int quantity)
    {
        var inventory = await _inventoryRepository.GetByProductIdAsync(productId);
        if (inventory == null || inventory.AvailableQuantity < quantity)
            return false;

        inventory.Quantity -= quantity;
        await _inventoryRepository.UpdateAsync(inventory);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _inventoryRepository.DeleteAsync(id);
    }
} 