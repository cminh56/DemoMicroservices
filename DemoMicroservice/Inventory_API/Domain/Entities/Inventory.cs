using System.ComponentModel.DataAnnotations;

namespace Inventory_API.Domain.Entities;

public class Inventory
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public int ReservedQuantity { get; set; } = 0;

    [Required]
    public int AvailableQuantity => Quantity - ReservedQuantity;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
} 