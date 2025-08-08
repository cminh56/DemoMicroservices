using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Product_API.Application.Services;
using Product_API.Domain.Entities;
using Product_API.Common.Constants;
using Product_API.Common;
using Product_API.Common.DTO;
using System.Linq;

namespace Product_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly IMapper _mapper;
        private readonly CategoryHttpClientService _categoryHttpClientService;
        private readonly InventoryGrpcClientService _inventoryService;

        public ProductsController(
            ProductService productService, 
            IMapper mapper, 
            CategoryHttpClientService categoryHttpClientService,
            InventoryGrpcClientService inventoryService)
        {
            _productService = productService;
            _mapper = mapper;
            _categoryHttpClientService = categoryHttpClientService;
            _inventoryService = inventoryService;
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _categoryHttpClientService.GetCategoriesAsync();
                if (categories == null)
                {
                    return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.ErrorSystem, "Unable to fetch categories from Catalog API."));
                }
                return Ok(new ApiResponse<IEnumerable<CategoryDTO>>(200, ResponseKeys.Success, categories));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.ErrorSystem, ex.Message));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId, [FromQuery] string? searchTerm, 
                                              [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
        {
            try
            {
                IEnumerable<Product> products;
                
                // Sử dụng filter nếu có tham số filter
                if (categoryId.HasValue || !string.IsNullOrWhiteSpace(searchTerm) || minPrice.HasValue || maxPrice.HasValue)
                {
                    products = await _productService.GetFilteredAsync(categoryId, searchTerm, minPrice, maxPrice);
                }
                else
                {
                    products = await _productService.GetAllAsync();
                }
                
                var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(products);
                
                // Lấy danh sách danh mục để điền tên danh mục
                var categories = await _categoryHttpClientService.GetCategoriesAsync();
                
                // Get product quantities from inventory service
                var productIds = products.Select(p => p.Id).ToList();
                var quantities = await _inventoryService.GetProductQuantitiesAsync(productIds);
                
                // Điền tên danh mục và số lượng cho từng sản phẩm
                foreach (var productDto in productDtos)
                {
                    // Set category name
                    if (categories != null)
                    {
                        var category = categories.FirstOrDefault(c => c.Id == productDto.CategoryID);
                        if (category != null)
                        {
                            productDto.CategoryName = category.Name;
                        }
                    }
                    
                    // Set quantity
                    if (quantities.TryGetValue(productDto.Id, out int quantity))
                    {
                        productDto.Quantity = quantity;
                    }
                    else
                    {
                        productDto.Quantity = 0;
                    }
                }
                
                return Ok(new ApiResponse<IEnumerable<ProductDTO>>(200, ResponseKeys.Success, productDtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.ErrorSystem, ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new ApiResponse<string>(404, ResponseKeys.NotFound, 
                        string.Format(AppConstants.Validation.ProductNotFound, id)));

                var productDto = _mapper.Map<ProductDTO>(product);
                
                // Lấy thông tin danh mục để điền tên danh mục
                var categories = await _categoryHttpClientService.GetCategoriesAsync();
                if (categories != null)
                {
                    var category = categories.FirstOrDefault(c => c.Id == productDto.CategoryID);
                    if (category != null)
                    {
                        productDto.CategoryName = category.Name;
                    }
                }
                
                // Get product quantity from inventory service
                productDto.Quantity = await _inventoryService.GetProductQuantityAsync(id);
                
                return Ok(new ApiResponse<ProductDTO>(200, ResponseKeys.Success, productDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.ErrorSystem, ex.Message));
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateProductDTO dto)
        {
            try
            {
                var product = _mapper.Map<Product>(dto);
                var createdProduct = await _productService.AddAsync(product);
                var productDto = _mapper.Map<ProductDTO>(createdProduct);
                
                // Lấy thông tin danh mục để điền tên danh mục
                var categories = await _categoryHttpClientService.GetCategoriesAsync();
                if (categories != null)
                {
                    var category = categories.FirstOrDefault(c => c.Id == productDto.CategoryID);
                    if (category != null)
                    {
                        productDto.CategoryName = category.Name;
                    }
                }
                
                // New products have 0 quantity by default
                productDto.Quantity = 0;
                
                return CreatedAtAction(nameof(GetById), new { id = productDto.Id }, 
                    new ApiResponse<ProductDTO>(201, ResponseKeys.Created, productDto));
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
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDTO dto)
        {
            try
            {
                var product = _mapper.Map<Product>(dto);
                var updatedProduct = await _productService.UpdateAsync(id, product);
                var productDto = _mapper.Map<ProductDTO>(updatedProduct);
                
                // Lấy thông tin danh mục để điền tên danh mục
                var categories = await _categoryHttpClientService.GetCategoriesAsync();
                if (categories != null)
                {
                    var category = categories.FirstOrDefault(c => c.Id == productDto.CategoryID);
                    if (category != null)
                    {
                        productDto.CategoryName = category.Name;
                    }
                }
                
                // Get product quantity from inventory service
                productDto.Quantity = await _inventoryService.GetProductQuantityAsync(id);
                
                return Ok(new ApiResponse<ProductDTO>(200, ResponseKeys.Updated, productDto));
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _productService.RemoveAsync(id);
                return Ok(new ApiResponse<string>(200, ResponseKeys.Deleted, null));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(404, ResponseKeys.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.ErrorSystem, ex.Message));
            }
        }
    }
}
