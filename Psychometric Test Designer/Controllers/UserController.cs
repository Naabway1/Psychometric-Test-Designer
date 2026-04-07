using Microsoft.AspNetCore.Mvc;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Services;
using Psychometric_Test_Designer.Data;

namespace Psychometric_Test_Designer.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

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

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById([FromRoute] int userId)
        {
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

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int userId)
        {
            var user = await _userService.DeleteUser(userId);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }
            return Ok(user);
        }

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
            var user = await _userService.UpdateUser(userId, dto);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }
            return Ok(user);
        }
        /* 
        [HttpGet("{userId}/metrics")]
        public async Task<IActionResult> GetMetrics(int userId)
        {
            var metrics = await _db.UserMetrics.Where(um => um.UserId == userId).Join(_db.Metrics,
            um => um.MetricId,
            m => m.MetricId,
            (um, m) => new UserMetricDto
                {
                    MetricId = m.MetricId,
                    MetricName = m.Name,
                    Value = um.UserMetricValue
                })
            .ToListAsync();

            return Ok(metrics);
        } 
        */
    }
}