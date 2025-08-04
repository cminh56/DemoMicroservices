using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Identity_API.Common.DTO.Role;
using Identity_API.Domain.Entities;
using Identity_API.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Identity_API.Application.Services
{
    public class RoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public RoleService(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<RoleDto> CreateRoleAsync(RoleDto roleDto)
        {
            if (string.IsNullOrWhiteSpace(roleDto.Name))
                throw new ArgumentException("Role name cannot be empty.");

            var existingRole = await _roleRepository.GetRoleByNameAsync(roleDto.Name);
            if (existingRole != null)
                throw new ArgumentException($"Role '{roleDto.Name}' already exists.");

            var role = _mapper.Map<IdentityRole>(roleDto);
            var result = await _roleRepository.CreateRoleAsync(role);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create role: {errors}");
            }

            return _mapper.Map<RoleDto>(role);
        }

        public async Task<RoleDto> UpdateRoleAsync(RoleDto roleDto)
        {
            if (string.IsNullOrWhiteSpace(roleDto.Id))
                throw new ArgumentException("Role ID is required.");

            var existingRole = await _roleRepository.GetRoleByIdAsync(roleDto.Id);
            if (existingRole == null)
                throw new KeyNotFoundException($"Role with ID {roleDto.Id} not found.");

            // Check if the new name is already taken by another role
            var roleWithSameName = await _roleRepository.GetRoleByNameAsync(roleDto.Name);
            if (roleWithSameName != null && roleWithSameName.Id != roleDto.Id)
                throw new ArgumentException($"Role name '{roleDto.Name}' is already taken.");

            existingRole.Name = roleDto.Name;
            existingRole.NormalizedName = roleDto.Name.ToUpper();
            
            var result = await _roleRepository.UpdateRoleAsync(existingRole);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to update role: {errors}");
            }

            return _mapper.Map<RoleDto>(existingRole);
        }

        public async Task<bool> DeleteRoleAsync(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentException("Role ID is required.");

            var role = await _roleRepository.GetRoleByIdAsync(roleId);
            if (role == null)
                return false;
                
            var result = await _roleRepository.DeleteRoleAsync(roleId);
            return result.Succeeded;
        }

        public async Task<RoleDto> GetRoleByIdAsync(string roleId)
        {
            var role = await _roleRepository.GetRoleByIdAsync(roleId);
            return _mapper.Map<RoleDto>(role);
        }

        public async Task<RoleDto> GetRoleByNameAsync(string roleName)
        {
            var role = await _roleRepository.GetRoleByNameAsync(roleName);
            return _mapper.Map<RoleDto>(role);
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllRolesAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles);
        }
    }
}
