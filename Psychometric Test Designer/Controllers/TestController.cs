using Microsoft.AspNetCore.Mvc;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Services;

namespace Psychometric_Test_Designer.Controllers
{
    [ApiController]
    [Route("api/tests")]
    public class TestController : ControllerBase
    {
        private readonly TestService _testService;
        public TestController(TestService testService)
        {
            _testService = testService;
        }

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

        [HttpGet]
        public async Task<IActionResult> GetAllTests()
        {
            var tests = await _testService.GetAllTests();
            return Ok(tests);
        }

        [HttpGet("{testId}")]
        public async Task<IActionResult> GetTestById([FromRoute] int testId)
        {
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
    }
}