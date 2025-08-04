using System.ComponentModel.DataAnnotations;

namespace Identity_API.Common.DTO.Role
{
    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
    }
}
