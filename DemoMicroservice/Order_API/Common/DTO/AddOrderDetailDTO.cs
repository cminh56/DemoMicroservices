using System;

namespace Order_API.Common.DTO
{
    public class AddOrderDetailDTO
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
} 