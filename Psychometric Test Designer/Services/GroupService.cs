using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Models;

namespace Psychometric_Test_Designer.Services
{
    public class GroupService
    {
        public AppDbContext _db;

        public GroupService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Group>> GetAllGroups()
        {
            var groups = await _db.Groups.Select(g => new Group
            {
                GroupId = g.GroupId,
                GroupName = g.GroupName,
                Specialization = g.Specialization
            }).ToListAsync();

            return groups;
        }

        public async Task<Group> GetGroupByName(string groupName)
        {
            var group = await _db.Groups.FirstOrDefaultAsync(g => g.GroupName == groupName);
            if (group == null)
            {
                throw new Exception("Группа не найдена");
            }
            return group;
        }

        public async Task<Group?> CreateGroup(GroupDto dto)
        {
            var group = new Group
            {
                GroupName = dto.GroupName,
                Specialization = dto.Specialization
            };

            _db.Groups.Add(group);
            int result = await _db.SaveChangesAsync();
            return result > 0 ? group : null;
        }

        public async Task<Group?> DeleteGroup(string groupName)
        {
            var group = await _db.Groups.FirstOrDefaultAsync(g => g.GroupName == groupName);
            if (group == null)
            {
                return null;
            }

            _db.Groups.Remove(group);
            int result = await _db.SaveChangesAsync();
            return result > 0 ? group : null;
        }

        public async Task<Group?> UpdateGroup(string groupName, GroupDto dto)
        {
            var group = await _db.Groups.FirstOrDefaultAsync(g => g.GroupName == groupName);
            if (group == null)
            {
                return null;
            }

            if (group.GroupName != null)
            {
                group.GroupName = dto.GroupName;
            }

            if (group.Specialization != null)
            {
                group.Specialization = dto.Specialization;
            }

            _db.Groups.Update(group);
            int result = await _db.SaveChangesAsync();
            return result > 0 ? group : null;
        }
    }
}
