using Identity_API.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Identity_API.Domain.Interfaces
{
    public interface IRoleRepository : IDisposable
    {

        Task<IdentityResult> CreateRoleAsync(IdentityRole role);
        
        Task<IdentityResult> UpdateRoleAsync(IdentityRole role);
        
        Task<IdentityResult> DeleteRoleAsync(string roleId);

        Task<IdentityRole> GetRoleByIdAsync(string roleId);

        Task<IdentityRole> GetRoleByNameAsync(string roleName);
        Task<IEnumerable<IdentityRole>> GetAllRolesAsync();
    }
}
