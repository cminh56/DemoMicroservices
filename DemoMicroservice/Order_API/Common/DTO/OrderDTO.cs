using System;
using System.Collections.Generic;

namespace Order_API.Common.DTO
{
    public class OrderDTO
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderDetailDTO> OrderDetails { get; set; } = new();
    }
} 