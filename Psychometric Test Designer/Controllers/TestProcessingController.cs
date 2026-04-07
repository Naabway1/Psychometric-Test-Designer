using Microsoft.AspNetCore.Mvc;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Services;

namespace Psychometric_Test_Designer.Controllers
{
    [ApiController]
    [Route("api/test-processing")]
    public class TestProcessingController : ControllerBase
    {
        private readonly TestProcessingService _service;

        public TestProcessingController(TestProcessingService service)
        {
            _service = service;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitTestDto dto)
        {
            try
            {
                await _service.ProcessTest(dto);
                return Ok(new { message = "Тест обработан" });
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
    }
}
