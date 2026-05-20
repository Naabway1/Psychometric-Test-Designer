using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Psychometric_Test_Designer.Core;
using Psychometric_Test_Designer.Services;

namespace Psychometric_Test_Designer.Controllers
{
    [ApiController]
    [Authorize(Policy = "StaffOnly")]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly AnalyticsService _analyticsService;
        private readonly NotificationService _notificationService;

        public AnalyticsController(AnalyticsService analyticsService, NotificationService notificationService)
        {
            _analyticsService = analyticsService;
            _notificationService = notificationService;
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminAnalytics()
        {
            var result = await _analyticsService.GetAdminAnalytics();
            return Ok(result);
        }

        [HttpGet("fill-rates")]
        public async Task<IActionResult> GetFillRates()
        {
            var result = await _analyticsService.GetGroupFillRates();
            return Ok(result);
        }

        [HttpGet("alerts")]
        public async Task<IActionResult> GetAlerts()
        {
            var result = await _notificationService.GetActiveAlerts();
            return Ok(result);
        }

        [HttpGet("recommendations/{userId}")]
        public async Task<IActionResult> GetRecommendations([FromRoute] int userId)
        {
            try
            {
                var result = await _analyticsService.GetRecommendations(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
