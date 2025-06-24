namespace Basket_API.Common.DTO
{
    public class BasketDTO
    {
        public Guid UserId { get; set; }
        public List<BasketItemDTO> Items { get; set; } = new();
    }

    public class BasketItemDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
} 