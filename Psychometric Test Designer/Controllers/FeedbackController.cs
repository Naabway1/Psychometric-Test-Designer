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
    [Route("api/feedback")]
    public class FeedbackController : ControllerBase
    {
        private readonly FeedbackService _feedbackService;

        public FeedbackController(FeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [Authorize(Roles = UserRoles.Student)]
        [HttpPost("me")]
        public async Task<IActionResult> Submit([FromBody] FeedbackSubmitDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _feedbackService.Submit(userId.Value, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "PsychologistOnly")]
        [HttpGet("trends/all")]
        public async Task<IActionResult> GetAllTrends()
        {
            var result = await _feedbackService.GetAllTrends();
            return Ok(result);
        }

        [Authorize(Policy = "PsychologistOnly")]
        [HttpGet("groups/{groupId:int}/trends")]
        public async Task<IActionResult> GetGroupTrends([FromRoute] int groupId)
        {
            try
            {
                var result = await _feedbackService.GetGroupTrends(groupId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }
}
