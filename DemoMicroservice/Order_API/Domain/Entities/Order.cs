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
        public Guid UserID { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        // Navigation property for OrderDetails
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
} 