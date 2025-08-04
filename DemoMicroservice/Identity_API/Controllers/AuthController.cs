using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Identity_API.Application.Services;
using Identity_API.Common;
using Identity_API.Common.Constants;
using Identity_API.Common.DTO.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Identity_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthController(AuthService authService, IMapper mapper, IConfiguration configuration)
        {
            _authService = authService;
            _mapper = mapper;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _authService.RegisterAsync(request);
                var token = await _authService.GenerateJwtToken(user);
                var userProfile = _mapper.Map<UserProfileResponse>(user);
                
                var loginResponse = new LoginResponse
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpireHours"])),
                    User = userProfile
                };
                
                return CreatedAtAction(nameof(GetProfile), new { }, 
                    new ApiResponse<LoginResponse>(201, ResponseKeys.Created, loginResponse));
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _authService.LoginAsync(request.Email, request.Password);
                var token = await _authService.GenerateJwtToken(user);
                var userProfile = _mapper.Map<UserProfileResponse>(user);
                
                var loginResponse = new LoginResponse
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpireHours"])),
                    User = userProfile
                };
                
                return Ok(new ApiResponse<LoginResponse>(200, ResponseKeys.Success, loginResponse));
            }
            catch (ArgumentException ex)
            {
                return Unauthorized(new ApiResponse<string>(401, ResponseKeys.ValidationError, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(500, ResponseKeys.ErrorSystem, ex.Message));
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponse<string>(401, ResponseKeys.Unauthorized, "User not authenticated"));

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound(new ApiResponse<string>(404, ResponseKeys.NotFound, "User not found"));

                var userProfile = _mapper.Map<UserProfileResponse>(user);
                return Ok(new ApiResponse<UserProfileResponse>(200, ResponseKeys.Success, userProfile));
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
