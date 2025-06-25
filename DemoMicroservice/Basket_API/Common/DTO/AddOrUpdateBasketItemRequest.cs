namespace Basket_API.Common.DTO
{
    public class AddOrUpdateBasketItemRequest
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
} 