using Microsoft.AspNetCore.Mvc;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.DTOs.Auth;
using Psychometric_Test_Designer.Services;

namespace Psychometric_Test_Designer.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var auth = await _authService.Register(registerDto);
                return Ok(auth);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var auth = await _authService.Login(loginDto);
            if (auth == null)
            {
                return Unauthorized();
            }

            return Ok(auth);
        }

        [HttpPost("register-staff")]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffDto dto)
        {
            try
            {
                var auth = await _authService.RegisterStaff(dto);
                return Ok(auth);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
