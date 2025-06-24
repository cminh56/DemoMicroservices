namespace Inventory_API.Common.DTO
{
    public class InventoryDTO
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateInventoryDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateInventoryDTO
    {
        public int Quantity { get; set; }
    }

    public class GetQuantityDTO
    {
        public Guid ProductId { get; set; }
    }

    public class QuantityResponseDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
    }

    public class UpdateQuantityDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class GetQuantitiesDTO
    {
        public List<Guid> ProductIds { get; set; } = new();
    }

    public class QuantitiesResponseDTO
    {
        public List<QuantityResponseDTO> Items { get; set; } = new();
    }
}