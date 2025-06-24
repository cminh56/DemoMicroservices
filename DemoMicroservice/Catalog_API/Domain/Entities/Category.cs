using System.ComponentModel.DataAnnotations;

namespace Catalog_API.Domain.Entities
{
    public class Category
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(255)]
        public string? Description { get; set; }
    }
} 