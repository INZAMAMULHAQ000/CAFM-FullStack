using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CAFMSystem.API.Services;
using CAFMSystem.API.DTOs;
using System.Security.Claims;

namespace CAFMSystem.API.Controllers
{
    /// <summary>
    /// Controller for authentication operations (login, register, user management)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="registerDto">Registration details</param>
        /// <returns>Authentication response with user details</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid input data."
                    });
                }

                var result = await _authService.RegisterAsync(registerDto);
                
                if (result.Success)
                {
                    _logger.LogInformation($"User {registerDto.Email} registered successfully");
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "An internal error occurred."
                });
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>Authentication response with JWT token</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid input data."
                    });
                }

                var result = await _authService.LoginAsync(loginDto);
                
                if (result.Success)
                {
                    _logger.LogInformation($"User {loginDto.Email} logged in successfully");
                    return Ok(result);
                }

                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "An internal error occurred."
                });
            }
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <returns>Current user details</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Get users by role (for assignment purposes)
        /// </summary>
        /// <param name="role">Role name</param>
        /// <returns>List of users in the specified role</returns>
        [HttpGet("users/role/{role}")]
        [Authorize(Roles = "Admin,AssetManager")]
        public async Task<ActionResult<List<UserDto>>> GetUsersByRole(string role)
        {
            try
            {
                var users = await _authService.GetUsersByRoleAsync(role);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting users by role {role}");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Get all available roles
        /// </summary>
        /// <returns>List of available roles</returns>
        [HttpGet("roles")]
        public ActionResult<List<string>> GetRoles()
        {
            var roles = new List<string>
            {
                "Admin",
                "AssetManager", 
                "Plumber",
                "Electrician",
                "Cleaner",
                "EndUser"
            };

            return Ok(roles);
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        /// <returns>Token validation result</returns>
        [HttpGet("validate-token")]
        [Authorize]
        public ActionResult ValidateToken()
        {
            return Ok(new { valid = true, message = "Token is valid" });
        }
    }
}
