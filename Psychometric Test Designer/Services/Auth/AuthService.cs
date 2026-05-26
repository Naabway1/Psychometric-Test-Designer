using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.DTOs.Auth;
using Psychometric_Test_Designer.Core;
using Psychometric_Test_Designer.Models;

namespace Psychometric_Test_Designer.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly PasswordService _passwordService;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthService(
            AppDbContext db,
            PasswordService passwordService,
            JwtService jwtService,
            IConfiguration configuration)
        {
            _db = db;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> Register(RegisterDto registerDto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            var existsingUser = await _db.Users.AnyAsync(u => u.Login == registerDto.Login);
            if (existsingUser)
            {
                throw new Exception("Юзер с таким логином уже существует");
            }

            if (string.IsNullOrWhiteSpace(registerDto.FullName))
            {
                throw new Exception("Укажите ФИО");
            }

            var existingToken = await _db.Tokens.FirstOrDefaultAsync(t => t.TokenId == registerDto.Token);

            if (existingToken == null)
            {
                throw new Exception("Токен регистрации не найден:");
            }

            if (existingToken.NumberOfUses <= 0)
            {
                throw new Exception("Токен исчерпан");
            }

            existingToken.NumberOfUses -= 1;

            var user = new User
            {
                Login = registerDto.Login,
                FullName = registerDto.FullName.Trim(),
                Password = _passwordService.HashPassword(registerDto.Password),
                GroupId = existingToken.GroupId,
                Role = UserRoles.Student
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return await CreateAuthResponse(user);
        }

        public async Task<AuthResponseDto?> Login(LoginDto loginDto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == loginDto.Login);
            if (user == null) { return null; }
            var valid = _passwordService.VerifyPassword(loginDto.Password, user.Password);
            if (!valid) { return null; }
            return await CreateAuthResponse(user);
        }

        public async Task<AuthResponseDto> RegisterStaff(RegisterStaffDto dto)
        {
            var configuredKey = _configuration["AdminBootstrapKey"] ?? "change-me-admin-bootstrap-key";
            if (dto.BootstrapKey != configuredKey)
            {
                throw new Exception("Некорректный ключ создания сотрудника");
            }

            var role = NormalizeStaffRole(dto.Role);
            var existingUser = await _db.Users.AnyAsync(u => u.Login == dto.Login);
            if (existingUser)
            {
                throw new Exception("Юзер с таким логином уже существует");
            }

            if (string.IsNullOrWhiteSpace(dto.FullName))
            {
                throw new Exception("Укажите ФИО сотрудника");
            }

            var groupId = dto.GroupId > 0 ? dto.GroupId : await GetOrCreateStaffGroupId();
            var groupExists = await _db.Groups.AnyAsync(g => g.GroupId == groupId);
            if (!groupExists)
            {
                throw new Exception("Группа не найдена");
            }

            var user = new User
            {
                Login = dto.Login,
                FullName = dto.FullName.Trim(),
                Password = _passwordService.HashPassword(dto.Password),
                GroupId = groupId,
                Role = role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return await CreateAuthResponse(user);
        }

        private async Task<int> GetOrCreateStaffGroupId()
        {
            const string staffGroupName = "ADM";
            var group = await _db.Groups.FirstOrDefaultAsync(g => g.GroupName == staffGroupName);
            if (group != null)
            {
                return group.GroupId;
            }

            group = new Group
            {
                GroupName = staffGroupName,
                Specialization = "Сотрудники",
                StudentCount = 0
            };

            _db.Groups.Add(group);
            await _db.SaveChangesAsync();
            return group.GroupId;
        }

        private async Task<AuthResponseDto> CreateAuthResponse(User user)
        {
            var groupName = user.Group?.GroupName
                ?? await _db.Groups
                    .Where(group => group.GroupId == user.GroupId)
                    .Select(group => group.GroupName)
                    .FirstOrDefaultAsync()
                ?? user.GroupId.ToString();

            return new AuthResponseDto
            {
                UserId = user.UserId,
                GroupId = user.GroupId,
                GroupName = groupName,
                FullName = user.FullName ?? string.Empty,
                Role = user.Role,
                Token = _jwtService.Generate(user)
            };
        }

        private static string NormalizeStaffRole(string role)
        {
            if (string.Equals(role, UserRoles.Psychologist, StringComparison.OrdinalIgnoreCase))
            {
                return UserRoles.Psychologist;
            }

            if (string.Equals(role, UserRoles.SocialTeacher, StringComparison.OrdinalIgnoreCase))
            {
                return UserRoles.SocialTeacher;
            }

            return UserRoles.Admin;
        }
    }
}
