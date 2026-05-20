using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Psychometric_Test_Designer.Core;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Services;

namespace Psychometric_Test_Designer.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserDto dto)
        {
            var user = await _userService.CreateUser(dto);
            if (user == null)
            {
                return BadRequest("Не удалось создать пользователя");
            }

            return Ok(user);
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            return await GetUserById(userId.Value);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById([FromRoute] int userId)
        {
            if (!CanAccessUser(userId))
            {
                return Forbid();
            }

            try
            {
                var user = await _userService.GetUserById(userId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int userId)
        {
            var user = await _userService.DeleteUser(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            return Ok(user);
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetUsersByGroupId([FromRoute] int groupId)
        {
            try
            {
                var users = await _userService.GetUsersByGroupId(groupId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpGet("group/name/{groupName}")]
        public async Task<IActionResult> GetUsersByGroupName([FromRoute] string groupName)
        {
            try
            {
                var users = await _userService.GetUsersByGroupName(groupName);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpGet("login/{login}")]
        public async Task<IActionResult> GetUserByLogin([FromRoute] string login)
        {
            try
            {
                var user = await _userService.GetUserByLogin(login);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPatch("{userId}")]
        public async Task<IActionResult> UpdateUser([FromRoute] int userId, [FromBody] UserDto dto)
        {
            if (!CanAccessUser(userId))
            {
                return Forbid();
            }

            var user = await _userService.UpdateUser(userId, dto);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }

            return Ok(user);
        }

        [HttpGet("me/metrics")]
        public async Task<IActionResult> GetMyMetrics()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            return await GetMetrics(userId.Value);
        }

        [HttpGet("{userId}/metrics")]
        public async Task<IActionResult> GetMetrics([FromRoute] int userId)
        {
            if (!CanAccessUser(userId))
            {
                return Forbid();
            }

            try
            {
                var metrics = await _userService.GetUserMetrics(userId);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("me/scale-results")]
        public async Task<IActionResult> GetMyScaleResults()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            return await GetScaleResults(userId.Value);
        }

        [HttpGet("{userId}/scale-results")]
        public async Task<IActionResult> GetScaleResults([FromRoute] int userId)
        {
            if (!CanAccessUser(userId))
            {
                return Forbid();
            }

            try
            {
                var scaleResults = await _userService.GetUserScaleResults(userId);
                return Ok(scaleResults);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        private bool CanAccessUser(int userId)
        {
            return IsStaff() || GetCurrentUserId() == userId;
        }

        private bool IsStaff()
        {
            return User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.SocialTeacher);
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }
}
