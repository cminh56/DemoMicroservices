using Inventory_API.Domain.Entities;

namespace Inventory_API.Domain.Interfaces;

public interface IInventoryRepository
{
    Task<IEnumerable<Inventory>> GetAllAsync();
    Task<Inventory?> GetByIdAsync(Guid id);
    Task<Inventory?> GetByProductIdAsync(Guid productId);
    Task<IEnumerable<Inventory>> GetByProductIdsAsync(IEnumerable<Guid> productIds);
    Task<Inventory> CreateAsync(Inventory inventory);
    Task<Inventory> UpdateAsync(Inventory inventory);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByProductIdAsync(Guid productId);
}
