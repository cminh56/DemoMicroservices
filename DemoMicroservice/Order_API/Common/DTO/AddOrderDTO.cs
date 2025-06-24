using System;

namespace Order_API.Common.DTO
{
    public class AddOrderDTO
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? Note { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
} 