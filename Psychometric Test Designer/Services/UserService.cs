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
    }
}
