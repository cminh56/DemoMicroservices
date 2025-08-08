using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Order_API.Application.Services;
using Order_API.Domain.Entities;
using Order_API.Common.Constants;
using Order_API.Common.Helpers;
using Order_API.Common.DTO;

namespace Order_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderDetailController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly IMapper _mapper;

        public OrderDetailController(OrderService orderService, IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var details = await _orderService.GetAllOrderDetailsAsync();
                var dtos = _mapper.Map<IEnumerable<OrderDetailDTO>>(details);
                return Ok(new ApiResponse<IEnumerable<OrderDetailDTO>>(200, ResponseKeys.Success, dtos));
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
                var detail = await _orderService.GetOrderDetailByIdAsync(id);
                if (detail == null)
                    return NotFound(new ApiResponse<string>(404, ResponseKeys.Error, $"OrderDetail {id} not found"));
                var dto = _mapper.Map<OrderDetailDTO>(detail);
                return Ok(new ApiResponse<OrderDetailDTO>(200, ResponseKeys.Success, dto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] AddOrderDetailDTO dto)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(dto.OrderId);
                if (order == null)
                    return NotFound(new ApiResponse<string>(404, ResponseKeys.Error, $"Order {dto.OrderId} not found"));

                var detail = _mapper.Map<OrderDetail>(dto);
                var created = await _orderService.AddOrderDetailAsync(detail);
                var resultDto = _mapper.Map<OrderDetailDTO>(created);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, new ApiResponse<OrderDetailDTO>(201, ResponseKeys.Success, resultDto));
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderDetailDTO dto)
        {
            try
            {
                var detail = _mapper.Map<OrderDetail>(dto);
                var updated = await _orderService.UpdateOrderDetailAsync(id, detail);
        
                var resultDto = _mapper.Map<OrderDetailDTO>(updated);
                return Ok(new ApiResponse<OrderDetailDTO>(200, ResponseKeys.Success, resultDto));
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _orderService.DeleteOrderDetailAsync(id);
                return Ok(new ApiResponse<string>(200, ResponseKeys.Success, null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }
    }
} 