using Microsoft.AspNetCore.Mvc;
using Psychometric_Test_Designer.Services;

namespace Psychometric_Test_Designer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] DTOs.RegisterDto registerDto)
        {
            try
            {
                await _authService.Register(registerDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    inner2 = ex.InnerException?.InnerException?.Message
                });
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DTOs.LoginDto loginDto)
        {
            var userId = await _authService.Login(loginDto);
            if (userId == null)
            {
                return Unauthorized();
            }
            return Ok(new { userId });
        }
    }
}
