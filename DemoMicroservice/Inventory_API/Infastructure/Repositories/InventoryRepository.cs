using Microsoft.EntityFrameworkCore;
using Inventory_API.Domain.Entities;
using Inventory_API.Domain.Interfaces;
using Inventory_API.Infastructure.DataContext;

namespace Inventory_API.Infastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _context;

    public InventoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Inventory>> GetAllAsync()
    {
        return await _context.Inventories.ToListAsync();
    }

    public async Task<Inventory?> GetByIdAsync(Guid id)
    {
        return await _context.Inventories.FindAsync(id);
    }

    public async Task<Inventory?> GetByProductIdAsync(Guid productId)
    {
        return await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
    }

    public async Task<IEnumerable<Inventory>> GetByProductIdsAsync(IEnumerable<Guid> productIds)
    {
        return await _context.Inventories
            .Where(i => productIds.Contains(i.ProductId))
            .ToListAsync();
    }

    public async Task<Inventory> CreateAsync(Inventory inventory)
    {
        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }

    public async Task<Inventory> UpdateAsync(Inventory inventory)
    {
        inventory.UpdatedAt = DateTime.UtcNow;
        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var inventory = await _context.Inventories.FindAsync(id);
        if (inventory == null)
            return false;

        _context.Inventories.Remove(inventory);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Inventories.AnyAsync(i => i.Id == id);
    }

    public async Task<bool> ExistsByProductIdAsync(Guid productId)
    {
        return await _context.Inventories.AnyAsync(i => i.ProductId == productId);
    }
} 