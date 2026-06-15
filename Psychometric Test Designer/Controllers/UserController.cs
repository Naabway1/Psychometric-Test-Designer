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

        [Authorize(Policy = "AdminOnly")]
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

        [Authorize(Policy = "AdminOnly")]
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

        [Authorize(Policy = "AdminOnly")]
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

        [Authorize(Policy = "AdminOnly")]
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

        [Authorize(Policy = "AdminOnly")]
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

        [Authorize(Policy = "AdminOnly")]
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
            if (!CanAccessStudentResults(userId))
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

        [HttpGet("me/latest-test-result")]
        public async Task<IActionResult> GetMyLatestTestResult()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _userService.GetLatestTestResult(userId.Value);
                return Ok(result ?? new ProcessTestResultDto { UserId = userId.Value });
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{userId}/scale-results")]
        public async Task<IActionResult> GetScaleResults([FromRoute] int userId)
        {
            if (!CanAccessStudentResults(userId))
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

        [Authorize(Policy = "PsychologistOnly")]
        [HttpGet("groups/{groupId:int}/results")]
        public async Task<IActionResult> GetGroupStudentResults([FromRoute] int groupId)
        {
            try
            {
                var result = await _userService.GetGroupStudentResults(groupId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        private bool CanAccessUser(int userId)
        {
            return User.IsInRole(UserRoles.Admin)
                || User.IsInRole(UserRoles.Psychologist)
                || GetCurrentUserId() == userId;
        }

        private bool CanAccessStudentResults(int userId)
        {
            return User.IsInRole(UserRoles.Psychologist) || GetCurrentUserId() == userId;
        }

        private bool IsStaff()
        {
            return User.IsInRole(UserRoles.Admin)
                || User.IsInRole(UserRoles.Psychologist)
                || User.IsInRole(UserRoles.SocialTeacher);
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }
}
