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
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly IMapper _mapper;

        public OrderController(OrderService orderService, IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var orders = await _orderService.GetAllAsync();
                var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);
                return Ok(new ApiResponse<IEnumerable<OrderDTO>>(200, ResponseKeys.Success, orderDtos));
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
                var order = await _orderService.GetByIdAsync(id);
                if (order == null)
                    return NotFound(new ApiResponse<string>(404, ResponseKeys.Error, string.Format(AppConstants.Validation.OrderNotFound, id)));

                var orderDto = _mapper.Map<OrderDTO>(order);
                return Ok(new ApiResponse<OrderDTO>(200, ResponseKeys.Success, orderDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }

        [HttpGet("{orderId}/details")]
        public async Task<IActionResult> GetOrderDetails(Guid orderId)
        {
            try
            {
                var orderDetails = await _orderService.GetOrderDetailsByOrderIdAsync(orderId);
                var orderDetailsDtos = _mapper.Map<IEnumerable<OrderDetailDTO>>(orderDetails);
                return Ok(new ApiResponse<IEnumerable<OrderDetailDTO>>(200, ResponseKeys.Success, orderDetailsDtos));
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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] AddOrderDTO dto)
        {
            try
            {
                if (dto.UserID == Guid.Empty)
                    return BadRequest(new ApiResponse<string>(400, ResponseKeys.Error, AppConstants.Validation.RequiredUserID));
                if (string.IsNullOrWhiteSpace(dto.PaymentMethod))
                    return BadRequest(new ApiResponse<string>(400, ResponseKeys.Error, AppConstants.Validation.RequiredPaymentMethod));

                var order = _mapper.Map<Order>(dto);
                order.OrderDate = DateTime.UtcNow;
                var createdOrder = await _orderService.AddAsync(order);
                var orderDto = _mapper.Map<OrderDTO>(createdOrder);
                return CreatedAtAction(nameof(GetById), new { id = orderDto.Id }, new ApiResponse<OrderDTO>(201, ResponseKeys.Success, orderDto));
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderDTO dto)
        {
            try
            {
                var order = _mapper.Map<Order>(dto);
                var updatedOrder = await _orderService.UpdateAsync(id, order);
                var orderDto = _mapper.Map<OrderDTO>(updatedOrder);
                return Ok(new ApiResponse<OrderDTO>(200, ResponseKeys.Success, orderDto));
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _orderService.RemoveAsync(id);
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