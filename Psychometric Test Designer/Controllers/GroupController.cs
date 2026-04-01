

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Services;

namespace Psychometric_Test_Designer.Controllers
{
    [ApiController]
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
                Specialization = g.Specialization
            }).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("{groupName}")]
        public async Task<IActionResult> GetGroupByName([FromRoute] string groupName)
        {
            var group = await _groupService.GetGroupByName(groupName);
            return Ok(new GroupDto
            {
                GroupName = group.GroupName,
                Specialization = group.Specialization
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

        [HttpDelete]
        [Route("{groupName}")]
        public async Task<IActionResult> DeleteGroupByName([FromRoute] string groupName)
        {
            var result = await _groupService.DeleteGroup(groupName);
            if (result == null)
            {
                return NotFound("Группа не найдена");
            }
            return Ok(result);
        }

        [HttpPatch]
        [Route("{groupName}")]
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