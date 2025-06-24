namespace Basket_API.Domain.Entities
{
    public class Basket
    {
        public Guid UserId { get; set; }
        public List<BasketItem> Items { get; set; } = new();
    }

    public class BasketItem
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
} 