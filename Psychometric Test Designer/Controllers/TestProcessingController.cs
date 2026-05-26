using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Psychometric_Test_Designer.Core;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Services;

namespace Psychometric_Test_Designer.Controllers
{
    [ApiController]
    [Route("api/test-processing")]
    public class TestProcessingController : ControllerBase
    {
        private readonly TestProcessingService _service;
        private readonly TestService _testService;

        public TestProcessingController(TestProcessingService service, TestService testService)
        {
            _service = service;
            _testService = testService;
        }

        [Authorize(Roles = UserRoles.Student)]
        [HttpPost("submit/me")]
        public async Task<IActionResult> SubmitCurrentUser([FromBody] SubmitCurrentUserTestDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            if (!await _testService.IsTestAvailableForUser(userId.Value, dto.TestId))
            {
                return BadRequest(new { message = "Тест сейчас недоступен для вашей группы" });
            }

            return await SubmitInternal(new SubmitTestDto
            {
                UserId = userId.Value,
                TestId = dto.TestId,
                Answers = dto.Answers
            });
        }

        [Authorize(Policy = "PsychologistOnly")]
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitTestDto dto)
        {
            return await SubmitInternal(dto);
        }

        private async Task<IActionResult> SubmitInternal(SubmitTestDto dto)
        {
            try
            {
                var result = await _service.ProcessTest(dto);
                return Ok(result);
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

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }
}
