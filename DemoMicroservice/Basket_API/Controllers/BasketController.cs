using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AutoMapper;
using Basket_API.Application.Services;
using Basket_API.Domain.Entities;
using Basket_API.Common.DTO;
using Basket_API.Common.Helpers;
using Basket_API.Common.Constants;
using System.Linq;

namespace Basket_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints
    public class BasketController : ControllerBase
    {
        private readonly BasketService _basketService;
        private readonly IMapper _mapper;
        private readonly BasketCheckoutPublisher _publisher;
        public BasketController(BasketService basketService, IMapper mapper, BasketCheckoutPublisher publisher)
        {
            _basketService = basketService;
            _mapper = mapper;
            _publisher = publisher;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var baskets = await _basketService.GetAllAsync();
            var basketDtos = _mapper.Map<IEnumerable<BasketDTO>>(baskets);
            return Ok(new ApiResponse<IEnumerable<BasketDTO>>(200, ResponseKeys.Success, basketDtos));
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetById(Guid userId)
        {
            try
            {
                var basket = await _basketService.GetByIdAsync(userId);
                if (basket == null)
                    return NotFound(new ApiResponse<string>(404, ResponseKeys.NotFound, string.Format(AppConstants.Validation.BasketNotFound, userId)));
                var basketDto = _mapper.Map<BasketDTO>(basket);
                return Ok(new ApiResponse<BasketDTO>(200, ResponseKeys.Success, basketDto));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<string>(400, ResponseKeys.ValidationError, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BasketDTO dto)
        {
            try
            {
                var basket = _mapper.Map<Basket>(dto);
                var createdBasket = await _basketService.AddAsync(basket);
                var basketDto = _mapper.Map<BasketDTO>(createdBasket);
                return CreatedAtAction(nameof(GetById), new { userId = basketDto.UserId }, new ApiResponse<BasketDTO>(201, ResponseKeys.Created, basketDto));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<string>(400, ResponseKeys.ValidationError, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }

        [HttpPost("item")]
        public async Task<IActionResult> AddBasketItem([FromBody] AddOrUpdateBasketItemRequest request)
        {
            // Ensure the user is accessing their own basket
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != request.UserId.ToString() && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            try
            {
                var basket = await _basketService.GetByIdAsync(request.UserId) ?? new Basket_API.Domain.Entities.Basket { UserId = request.UserId };
                var item = basket.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
                if (item != null)
                {
                    item.Quantity += request.Quantity;
                }
                else
                {
                    basket.Items.Add(new Basket_API.Domain.Entities.BasketItem
                    {
                        ProductId = request.ProductId,
                        Quantity = request.Quantity
                    });
                }
                var updated = await _basketService.AddAsync(basket);
                var basketDto = _mapper.Map<BasketDTO>(updated);
                return Ok(new ApiResponse<BasketDTO>(200, ResponseKeys.Updated, basketDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }

        [HttpPut("item")]
        public async Task<IActionResult> UpdateBasketItem([FromBody] AddOrUpdateBasketItemRequest request)
        {
            // Ensure the user is accessing their own basket
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != request.UserId.ToString() && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            try
            {
                var basket = await _basketService.GetByIdAsync(request.UserId);
                if (basket == null)
                    return NotFound(new ApiResponse<string>(404, ResponseKeys.NotFound, "Basket not found."));
                var item = basket.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
                if (item == null)
                    return NotFound(new ApiResponse<string>(404, ResponseKeys.NotFound, "Item not found in basket."));

                item.Quantity = request.Quantity;
                var updated = await _basketService.AddAsync(basket);
                var basketDto = _mapper.Map<BasketDTO>(updated);
                return Ok(new ApiResponse<BasketDTO>(200, ResponseKeys.Updated, basketDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }

        [HttpDelete("item")]
        public async Task<IActionResult> DeleteBasketItem([FromQuery] Guid userId, [FromQuery] Guid productId)
        {
            // Ensure the user is accessing their own basket
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != userId.ToString() && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            try
            {
                var updatedBasket = await _basketService.DeleteBasketItemAsync(userId, productId);
                var basketDto = _mapper.Map<BasketDTO>(updatedBasket);
                return Ok(new ApiResponse<BasketDTO>(200, ResponseKeys.Updated, basketDto));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<string>(400, ResponseKeys.ValidationError, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }

        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")] // Only admin can delete a basket
        public async Task<IActionResult> Delete(Guid userId)
        {
            try
            {
                await _basketService.DeleteAsync(userId);
                return Ok(new ApiResponse<string>(200, ResponseKeys.Deleted, null));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<string>(400, ResponseKeys.ValidationError, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.Error, ex.Message));
            }
        }

        [HttpPost("checkout")]
        [Authorize] // Require authentication for checkout
        public IActionResult Checkout([FromBody] object basketCheckoutDto)
        {
            _publisher.Publish(basketCheckoutDto);
            return Ok(new { message = "Checkout message published to RabbitMQ" });
        }
    }
}