using System;

namespace Order_API.Common.DTO
{
    public class UpdateOrderDetailDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
} 