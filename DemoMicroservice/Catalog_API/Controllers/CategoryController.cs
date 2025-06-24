using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Catalog_API.Application.Services;
using Catalog_API.Domain.Entities;
using Catalog_API.Common.DTO;
using Catalog_API.Common.Constants;
using Catalog_API.Common.Helpers;

namespace Catalog_API.Controllers
{
    [ApiController]
    [Route("api/Catalog")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _service;
        private readonly IMapper _mapper;
        public CategoryController(CategoryService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _service.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<CategoryDTO>>(categories);
                return Ok(new ApiResponse<IEnumerable<CategoryDTO>>(200, ResponseKeys.Success, dtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var category = await _service.GetByIdAsync(id);
                if (category == null)
                    return NotFound(new ApiResponse<string>(404, ResponseKeys.Error, string.Format(AppConstants.Validation.CategoryNotFound, id)));
                var dto = _mapper.Map<CategoryDTO>(category);
                return Ok(new ApiResponse<CategoryDTO>(200, ResponseKeys.Success, dto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryDTO dto)
        {
            try
            {
                var entity = _mapper.Map<Category>(dto);
                var created = await _service.AddAsync(entity);
                var resultDto = _mapper.Map<CategoryDTO>(created);
                return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, new ApiResponse<CategoryDTO>(201, ResponseKeys.Success, resultDto));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<string>(400, ResponseKeys.Error, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryDTO dto)
        {
            try
            {
                var entity = _mapper.Map<Category>(dto);
                var updated = await _service.UpdateAsync(id, entity);
                var resultDto = _mapper.Map<CategoryDTO>(updated);
                return Ok(new ApiResponse<CategoryDTO>(200, ResponseKeys.Success, resultDto));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(404, ResponseKeys.Error, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<string>(400, ResponseKeys.Error, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.RemoveAsync(id);
                return Ok(new ApiResponse<string>(200, ResponseKeys.Success, null));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(404, ResponseKeys.Error, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }
    }
} 