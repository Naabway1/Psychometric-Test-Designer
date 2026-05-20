using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Psychometric_Test_Designer.Core;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Services;

namespace Psychometric_Test_Designer.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/tests")]
    public class TestController : ControllerBase
    {
        private readonly TestService _testService;
        public TestController(TestService testService)
        {
            _testService = testService;
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateTest([FromBody] TestDto dto)
        {
            var test = await _testService.CreateTest(dto);
            if (test == null)
            {
                return BadRequest("Не удалось создать тест");
            }
            return Ok(test);
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpPost("full")]
        public async Task<IActionResult> CreateFullTest([FromBody] CreateFullTestDto dto)
        {
            try
            {
                var test = await _testService.CreateFullTest(dto);
                return Ok(test);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpGet]
        public async Task<IActionResult> GetAllTests()
        {
            var tests = await _testService.GetAllTests();
            return Ok(tests);
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpGet("assignments")]
        public async Task<IActionResult> GetAssignments()
        {
            var assignments = await _testService.GetAssignments();
            return Ok(assignments);
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpPost("assignments")]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateTestAssignmentDto dto)
        {
            try
            {
                var assignment = await _testService.CreateAssignment(dto);
                return Ok(assignment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = UserRoles.Student)]
        [HttpGet("available/me")]
        public async Task<IActionResult> GetMyAvailableTests()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                var tests = await _testService.GetAvailableTestsForUser(userId.Value);
                return Ok(tests);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{testId}/full")]
        public async Task<IActionResult> GetFullTestById([FromRoute] int testId)
        {
            if (!await CanAccessTest(testId))
            {
                return Forbid();
            }

            try
            {
                var test = await _testService.GetFullTestById(testId);
                return Ok(test);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{testId}")]
        public async Task<IActionResult> GetTestById([FromRoute] int testId)
        {
            if (!await CanAccessTest(testId))
            {
                return Forbid();
            }

            try
            {
                var test = await _testService.GetTestById(testId);
                return Ok(test);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("creator/{creatorId}")]
        public async Task<IActionResult> GetTestsByCreatorId([FromRoute] int creatorId)
        {
            try
            {
                var tests = await _testService.GetTestsByCreatorId(creatorId);
                return Ok(tests);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize(Policy = "StaffOnly")]
        [HttpDelete("{testId}")]
        public async Task<IActionResult> DeleteTest([FromRoute] int testId)
        {
            var deletedTest = await _testService.DeleteTest(testId);
            if (deletedTest == null)
            {
                return NotFound("Тест не найден");
            }
            return Ok(deletedTest);
        }

        private async Task<bool> CanAccessTest(int testId)
        {
            if (User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.SocialTeacher))
            {
                return true;
            }

            var userId = GetCurrentUserId();
            return userId.HasValue && await _testService.IsTestAvailableForUser(userId.Value, testId);
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }
}
