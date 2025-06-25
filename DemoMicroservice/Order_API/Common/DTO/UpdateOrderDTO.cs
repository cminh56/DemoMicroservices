using System;
using System.ComponentModel.DataAnnotations;

namespace Order_API.Common.DTO
{
    public class UpdateOrderDTO
    {
        [Required]
        public Guid UserID { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }
}