using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Identity_API.Application.Services;
using Identity_API.Common;
using Identity_API.Common.Constants;
using Identity_API.Common.DTO.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only users with Admin role can access these endpoints
    public class RolesController : ControllerBase
    {
        private readonly RoleService _roleService;
        private readonly IMapper _mapper;

        public RolesController(RoleService roleService, IMapper mapper)
        {
            _roleService = roleService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                return Ok(new ApiResponse<IEnumerable<RoleDto>>(200, ResponseKeys.Success, roles));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.ErrorSystem, ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                    return NotFound(new ApiResponse<string>(404, ResponseKeys.NotFound, "Role not found"));

                return Ok(new ApiResponse<RoleDto>(200, ResponseKeys.Success, role));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.ErrorSystem, ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleDto roleDto)
        {
            try
            {
                var result = await _roleService.CreateRoleAsync(roleDto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
                    new ApiResponse<RoleDto>(201, ResponseKeys.Created, result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<string>(400, ResponseKeys.ValidationError, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.ErrorSystem, ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] RoleDto roleDto)
        {
            try
            {
                if (id != roleDto.Id)
                {
                    return BadRequest(new ApiResponse<string>(400, ResponseKeys.ValidationError, 
                        "Role ID in the URL does not match the ID in the request body."));
                }

                var result = await _roleService.UpdateRoleAsync(roleDto);
                return Ok(new ApiResponse<RoleDto>(200, ResponseKeys.Updated, result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(404, ResponseKeys.NotFound, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<string>(400, ResponseKeys.ValidationError, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.ErrorSystem, ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _roleService.DeleteRoleAsync(id);
                if (!result)
                    return NotFound(new ApiResponse<string>(404, ResponseKeys.NotFound, "Role not found"));

                return Ok(new ApiResponse<string>(200, ResponseKeys.Deleted, null));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<string>(400, ResponseKeys.ValidationError, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.ErrorSystem, ex.Message));
            }
        }
    }
}
