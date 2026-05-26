using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Core;
using Psychometric_Test_Designer.Models;

namespace Psychometric_Test_Designer.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<User>> GetAllUsers()
        {
            var users = await _db.Users.Select(u => new User
            {
                UserId = u.UserId,
                Login = u.Login,
                FullName = u.FullName,
                Password = u.Password,
                Role = u.Role,
                GroupId = u.GroupId,
                CreatedAt = u.CreatedAt
            }).ToListAsync();

            return users;
        }

        public async Task<List<User>> GetUsersByGroupId(int groupId)
        {
            var users = await _db.Users.Where(u => u.GroupId == groupId).ToListAsync();
            if (users == null || users.Count == 0)
            {
                throw new Exception("Пользователи не найдены");
            }
            return users;
        }

        public async Task<List<User>> GetUsersByGroupName(string groupName)
        {
            var users = await _db.Users.Where(u => u.Group.GroupName == groupName).ToListAsync();
            if (users == null || users.Count == 0)
            {
                throw new Exception("Пользователи не найдены");
            }
            return users;
        }

        public async Task<User> GetUserById(int userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new Exception("Пользователь не найден");
            }
            return user;
        }

        public async Task<User?> GetUserByLogin(string login)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (user == null)
            {
                throw new Exception("Пользователь не найден");
            }
            return user;
        }

        public async Task<User?> CreateUser(UserDto dto)
        {
            var user = new User
            {
                Login = dto.Login,
                FullName = dto.FullName,
                Password = dto.Password,
                Role = UserRoles.Student,
                GroupId = dto.GroupId
            };

            _db.Users.Add(user);
            int result = await _db.SaveChangesAsync();
            return result > 0 ? user : null;
        }

        public async Task<User?> DeleteUser(int userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return null;
            }

            _db.Users.Remove(user);
            int result = await _db.SaveChangesAsync();
            return result > 0 ? user : null;
        }

        public async Task<User?> UpdateUser(int userId, UserDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return null;
            }

            if (user.Login != null)
            {
                user.Login = dto.Login;
            }
            if (user.FullName != null)
            {
                user.FullName = dto.FullName;
            }
            if (user.Password != null)
            {
                user.Password = dto.Password;
            }
            if (user.GroupId != 0)
            {
                user.GroupId = dto.GroupId;
            }

            int result = await _db.SaveChangesAsync();
            return result > 0 ? user : null;
        }

        public async Task<List<UserMetricDto>> GetUserMetrics(int userId)
        {
            var userExists = await _db.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                throw new Exception("Пользователь не найден");
            }

            return await _db.UserMetrics
                .Where(um => um.UserId == userId)
                .Select(um => new UserMetricDto
                {
                    MetricId = um.MetricId,
                    MetricName = um.Metric.Name,
                    IsPositive = um.Metric.IsPositive,
                    Value = um.Value
                })
                .OrderBy(um => um.MetricId)
                .ToListAsync();
        }

        public async Task<List<UserScaleResultDto>> GetUserScaleResults(int userId)
        {
            var userExists = await _db.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                throw new Exception("Пользователь не найден");
            }

            return await _db.UserScaleResults
                .Where(usr => usr.UserId == userId)
                .OrderByDescending(usr => usr.CreatedAt)
                .Select(usr => new UserScaleResultDto
                {
                    ScaleId = usr.ScaleId,
                    ScaleName = usr.Scale.Name,
                    IsPositive = usr.Scale.IsPositive,
                    RawScore = usr.RawScore,
                    NormalizedScore = usr.NormalizedScore,
                    SourceTestId = usr.SourceTestId,
                    CreatedAt = usr.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<StudentResultSummaryDto>> GetGroupStudentResults(int groupId)
        {
            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == groupId);
            if (!groupExists)
            {
                throw new Exception("Группа не найдена");
            }

            var students = await _db.Users
                .AsNoTracking()
                .Where(u => u.GroupId == groupId && u.Role == UserRoles.Student)
                .OrderBy(u => u.FullName)
                .Select(u => new
                {
                    u.UserId,
                    u.Login,
                    u.FullName,
                    u.GroupId,
                    GroupName = u.Group.GroupName
                })
                .ToListAsync();

            var userIds = students.Select(s => s.UserId).ToList();

            var metrics = await _db.UserMetrics
                .AsNoTracking()
                .Where(um => userIds.Contains(um.UserId))
                .Select(um => new
                {
                    um.UserId,
                    Metric = new UserMetricDto
                    {
                        MetricId = um.MetricId,
                        MetricName = um.Metric.Name,
                        IsPositive = um.Metric.IsPositive,
                        Value = um.Value
                    }
                })
                .ToListAsync();

            var scaleRows = await _db.UserScaleResults
                .AsNoTracking()
                .Where(usr => userIds.Contains(usr.UserId))
                .Select(usr => new
                {
                    usr.UserId,
                    usr.SourceTestId,
                    Scale = new UserScaleResultDto
                    {
                        ScaleId = usr.ScaleId,
                        ScaleName = usr.Scale.Name,
                        IsPositive = usr.Scale.IsPositive,
                        RawScore = usr.RawScore,
                        NormalizedScore = usr.NormalizedScore,
                        SourceTestId = usr.SourceTestId,
                        CreatedAt = usr.CreatedAt
                    }
                })
                .ToListAsync();

            return students.Select(student =>
            {
                var studentScales = scaleRows
                    .Where(row => row.UserId == student.UserId)
                    .ToList();
                var latestTestId = studentScales
                    .OrderByDescending(row => row.Scale.CreatedAt)
                    .Select(row => (int?)row.SourceTestId)
                    .FirstOrDefault();

                return new StudentResultSummaryDto
                {
                    UserId = student.UserId,
                    Login = student.Login,
                    FullName = string.IsNullOrWhiteSpace(student.FullName) ? student.Login : student.FullName,
                    GroupId = student.GroupId,
                    GroupName = student.GroupName,
                    LastActivityAt = studentScales
                        .OrderByDescending(row => row.Scale.CreatedAt)
                        .Select(row => (DateTime?)row.Scale.CreatedAt)
                        .FirstOrDefault(),
                    CurrentMetrics = metrics
                        .Where(row => row.UserId == student.UserId)
                        .Select(row => row.Metric)
                        .OrderBy(metric => metric.MetricId)
                        .ToList(),
                    LatestScales = latestTestId.HasValue
                        ? studentScales
                            .Where(row => row.SourceTestId == latestTestId.Value)
                            .Select(row => row.Scale)
                            .OrderBy(scale => scale.ScaleId)
                            .ToList()
                        : new List<UserScaleResultDto>()
                };
            }).ToList();
        }
    }
}
