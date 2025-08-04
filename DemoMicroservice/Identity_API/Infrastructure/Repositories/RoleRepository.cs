using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Identity_API.Domain.Entities;
using Identity_API.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity_API.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository, IDisposable
    {
        private bool _disposed = false;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleRepository(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IdentityResult> CreateRoleAsync(IdentityRole role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));
                
            return await _roleManager.CreateAsync(role);
        }

        public async Task<IdentityResult> UpdateRoleAsync(IdentityRole role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));
                
            var existingRole = await _roleManager.FindByIdAsync(role.Id);
            if (existingRole == null)
                throw new KeyNotFoundException($"Role with ID {role.Id} not found.");

            existingRole.Name = role.Name;
            existingRole.NormalizedName = role.Name.ToUpper();
            
            return await _roleManager.UpdateAsync(existingRole);
        }

        public async Task<IdentityResult> DeleteRoleAsync(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentException("Role ID cannot be null or empty.", nameof(roleId));
                
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {roleId} not found.");

            return await _roleManager.DeleteAsync(role);
        }

        public async Task<IdentityRole> GetRoleByIdAsync(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentException("Role ID cannot be null or empty.", nameof(roleId));
                
            return await _roleManager.FindByIdAsync(roleId);
        }

        public async Task<IdentityRole> GetRoleByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be null or empty.", nameof(roleName));
                
            return await _roleManager.FindByNameAsync(roleName);
        }

        public async Task<IEnumerable<IdentityRole>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.AsNoTracking().ToListAsync();
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                    _roleManager?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
