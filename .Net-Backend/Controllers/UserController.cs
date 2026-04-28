using Emart_DotNet.DTOs;
using Emart_DotNet.Models;
using Emart_DotNet.Services;
using Emart_DotNet.Utilities.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Emart_DotNet.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JwtHelper _jwtHelper;

        public UserController(IUserService userService, JwtHelper jwtHelper)
        {
            _userService = userService;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            try
            {
                var customer = await _userService.LoginAsync(request.Email, request.Password);
                string token = _jwtHelper.GenerateToken(customer);
                return Ok(new TokenResponse(token));
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] Customer customer)
        {
            try
            {
                var savedCustomer = await _userService.RegisterUserAsync(customer);
                string token = _jwtHelper.GenerateToken(savedCustomer);
                return Ok(new TokenResponse(token));
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromQuery] string email, [FromQuery] string fullName)
        {
             try
            {
                var customer = await _userService.ProcessGoogleLoginAsync(email, fullName);
                return Ok(Mappers.CustomerMapper.ToDTO(customer));
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("complete-registration/{userId}")]
        public async Task<IActionResult> CompleteRegistration(int userId, [FromBody] Customer customer)
        {
            try
            {
                var updatedCustomer = await _userService.CompleteRegistrationAsync(userId, customer);
                string token = _jwtHelper.GenerateToken(updatedCustomer);
                return Ok(new TokenResponse(token));
            }
             catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> ViewProfile(int userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                return Ok(Mappers.CustomerMapper.ToDTO(user));
            }
             catch (System.Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] Customer customer)
        {
            try
            {
                var updated = await _userService.UpdateUserAsync(userId, customer);
                return Ok(Mappers.CustomerMapper.ToDTO(updated));
            }
             catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
