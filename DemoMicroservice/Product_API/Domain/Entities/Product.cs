using System.ComponentModel.DataAnnotations;

namespace Product_API.Domain.Entities;

public class Product
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public Guid CategoryID { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
