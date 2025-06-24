using System;

namespace Order_API.Common.DTO
{
    public class UpdateOrderDTO
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string Status { get; set; } = string.Empty;
    }
} 