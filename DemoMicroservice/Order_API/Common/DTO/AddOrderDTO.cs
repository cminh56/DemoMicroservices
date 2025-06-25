using System;
using System.ComponentModel.DataAnnotations;

namespace Order_API.Common.DTO
{
    public class AddOrderDTO
    {
        [Required]
        public Guid UserID { get; set; }
        public decimal TotalAmount { get; set; }
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}