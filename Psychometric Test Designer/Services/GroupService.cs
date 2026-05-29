using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Core;
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
                Specialization = g.Specialization,
                StudentCount = g.StudentCount
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
                Specialization = dto.Specialization,
                StudentCount = dto.StudentCount
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

            if (dto.StudentCount.HasValue)
            {
                group.StudentCount = dto.StudentCount;
            }

            _db.Groups.Update(group);
            int result = await _db.SaveChangesAsync();
            return result > 0 ? group : null;
        }

        public async Task<List<GroupMetricDto>> GetGroupMetrics(int groupId)
        {
            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == groupId);
            if (!groupExists)
            {
                throw new Exception("Группа не найдена");
            }

            return await _db.UserMetrics
                .Where(um => um.User.GroupId == groupId && um.User.Role == UserRoles.Student)
                .GroupBy(um => new
                {
                    um.MetricId,
                    um.Metric.Name,
                    um.Metric.IsPositive
                })
                .Select(g => new GroupMetricDto
                {
                    MetricId = g.Key.MetricId,
                    MetricName = g.Key.Name,
                    IsPositive = g.Key.IsPositive,
                    AverageValue = g.Average(x => x.Value),
                    UsersCount = g.Select(x => x.UserId).Distinct().Count()
                })
                .OrderBy(m => m.MetricId)
                .ToListAsync();
        }

        public async Task<GroupRiskSummaryDto> GetGroupRiskSummary(int groupId)
        {
            var group = await _db.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.GroupId == groupId);
            if (group == null)
            {
                throw new Exception("Группа не найдена");
            }

            var usersCount = await _db.Users.CountAsync(u => u.GroupId == groupId && u.Role == UserRoles.Student);
            var metrics = await GetGroupMetrics(groupId);
            var riskIndex = metrics.Count == 0
                ? 0
                : metrics.Average(m => ToRiskScore(m.IsPositive, m.MetricName, m.AverageValue));

            return new GroupRiskSummaryDto
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                UsersCount = usersCount,
                RiskIndex = riskIndex,
                RiskLevel = GetRiskLevel(riskIndex),
                Metrics = metrics
            };
        }

        public async Task<List<GroupRiskSummaryDto>> GetAllGroupRiskSummaries()
        {
            var groupIds = await _db.Groups
                .Where(g => (g.StudentCount ?? 0) > 0)
                .OrderBy(g => g.GroupName)
                .Select(g => g.GroupId)
                .ToListAsync();

            var result = new List<GroupRiskSummaryDto>();
            foreach (var groupId in groupIds)
            {
                result.Add(await GetGroupRiskSummary(groupId));
            }

            return result
                .OrderByDescending(g => g.RiskIndex)
                .ThenBy(g => g.GroupName)
                .ToList();
        }

        public async Task<List<GroupMetricHistoryPointDto>> GetGroupMetricHistory(int groupId)
        {
            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == groupId);
            if (!groupExists)
            {
                throw new Exception("Группа не найдена");
            }

            return await _db.UserMetricSnapshots
                .Where(snapshot => snapshot.User.GroupId == groupId && snapshot.User.Role == UserRoles.Student)
                .GroupBy(snapshot => new
                {
                    snapshot.MetricId,
                    snapshot.Metric.Name,
                    snapshot.Metric.IsPositive,
                    Date = snapshot.CreatedAt.Date
                })
                .Select(g => new GroupMetricHistoryPointDto
                {
                    MetricId = g.Key.MetricId,
                    MetricName = g.Key.Name,
                    IsPositive = g.Key.IsPositive,
                    Date = g.Key.Date,
                    AverageValue = g.Average(x => x.Value)
                })
                .OrderBy(point => point.Date)
                .ThenBy(point => point.MetricId)
                .ToListAsync();
        }

        public async Task<List<GroupScaleDistributionDto>> GetGroupScaleDistribution(int groupId)
        {
            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == groupId);
            if (!groupExists)
            {
                throw new Exception("Группа не найдена");
            }

            var scaleResults = await _db.UserScaleResults
                .AsNoTracking()
                .Where(result => result.User.GroupId == groupId && result.User.Role == UserRoles.Student)
                .Select(result => new
                {
                    result.ScaleId,
                    ScaleName = result.Scale.Name,
                    IsPositive = result.Scale.IsPositive,
                    Score = result.NormalizedScore * 100
                })
                .ToListAsync();

            return scaleResults
                .GroupBy(result => new { result.ScaleId, result.ScaleName, result.IsPositive })
                .Select(g => new GroupScaleDistributionDto
                {
                    ScaleId = g.Key.ScaleId,
                    ScaleName = g.Key.ScaleName,
                    IsPositive = g.Key.IsPositive,
                    AverageValue = g.Average(x => x.Score),
                    LowCount = g.Count(x => x.Score < 40),
                    MediumCount = g.Count(x => x.Score >= 40 && x.Score < 70),
                    HighCount = g.Count(x => x.Score >= 70)
                })
                .OrderBy(result => result.ScaleId)
                .ToList();
        }

        private static decimal ToRiskScore(bool isPositive, string metricName, decimal value)
        {
            if (isPositive)
            {
                return 100m - value;
            }

            var name = metricName.ToLowerInvariant();

            if (name.Contains("благополуч")
                || name.Contains("настро")
                || name.Contains("стабил")
                || name.Contains("wellbeing")
                || name.Contains("mood")
                || name.Contains("stability"))
            {
                return 100m - value;
            }

            return value;
        }

        private static string GetRiskLevel(decimal riskIndex)
        {
            if (riskIndex >= 75)
            {
                return "critical";
            }

            if (riskIndex >= 60)
            {
                return "high";
            }

            if (riskIndex >= 40)
            {
                return "medium";
            }

            return "low";
        }
    }
}
