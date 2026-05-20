using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Services;

namespace Psychometric_Test_Designer.Controllers
{
    [ApiController]
    [Authorize(Policy = "StaffOnly")]
    [Route("api/groups")]
    public class GroupController : ControllerBase
    {
        private readonly GroupService _groupService;

        public GroupController(GroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGroups()
        {
            var groups = await _groupService.GetAllGroups();
            var result = groups.Select(g => new GroupDto
            {
                GroupName = g.GroupName,
                Specialization = g.Specialization,
                StudentCount = g.StudentCount
            }).ToList();

            return Ok(result);
        }

        [HttpGet("risk-summary")]
        public async Task<IActionResult> GetAllGroupRiskSummaries()
        {
            var summaries = await _groupService.GetAllGroupRiskSummaries();
            return Ok(summaries);
        }

        [HttpGet("{groupId:int}/metrics")]
        public async Task<IActionResult> GetGroupMetrics([FromRoute] int groupId)
        {
            try
            {
                var metrics = await _groupService.GetGroupMetrics(groupId);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{groupId:int}/metric-history")]
        public async Task<IActionResult> GetGroupMetricHistory([FromRoute] int groupId)
        {
            try
            {
                var history = await _groupService.GetGroupMetricHistory(groupId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{groupId:int}/scale-distribution")]
        public async Task<IActionResult> GetGroupScaleDistribution([FromRoute] int groupId)
        {
            try
            {
                var distribution = await _groupService.GetGroupScaleDistribution(groupId);
                return Ok(distribution);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{groupId:int}/risk-summary")]
        public async Task<IActionResult> GetGroupRiskSummary([FromRoute] int groupId)
        {
            try
            {
                var summary = await _groupService.GetGroupRiskSummary(groupId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{groupName}")]
        public async Task<IActionResult> GetGroupByName([FromRoute] string groupName)
        {
            var group = await _groupService.GetGroupByName(groupName);
            return Ok(new GroupDto
            {
                GroupName = group.GroupName,
                Specialization = group.Specialization,
                StudentCount = group.StudentCount
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] GroupDto dto)
        {
            var result = await _groupService.CreateGroup(dto);
            if (result == null)
            {
                return BadRequest("Ошибка при создании группы");
            }

            return Ok(result);
        }

        [HttpDelete("{groupName}")]
        public async Task<IActionResult> DeleteGroupByName([FromRoute] string groupName)
        {
            var result = await _groupService.DeleteGroup(groupName);
            if (result == null)
            {
                return NotFound("Группа не найдена");
            }

            return Ok(result);
        }

        [HttpPatch("{groupName}")]
        public async Task<IActionResult> UpdateGroup([FromRoute] string groupName, [FromBody] GroupDto dto)
        {
            var result = await _groupService.UpdateGroup(groupName, dto);
            if (result == null)
            {
                return NotFound("Группа не найдена");
            }

            return Ok(result);
        }
    }
}
