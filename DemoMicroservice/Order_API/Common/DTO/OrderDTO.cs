using System;
using System.Collections.Generic;

namespace Order_API.Common.DTO
{
    public class OrderDTO
    {
        public Guid Id { get; set; }
        public Guid UserID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<OrderDetailDTO> OrderDetails { get; set; } = new();
    }
}