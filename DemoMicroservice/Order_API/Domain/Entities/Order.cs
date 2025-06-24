using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Order_API.Domain.Entities
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public string? Note { get; set; }

        public string Status { get; set; } = string.Empty;

        // Navigation property for OrderDetails
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
} 