using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Models;

namespace Psychometric_Test_Designer.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly PasswordService _passwordService;

        public AuthService(AppDbContext db, PasswordService passwordService)
        {
            _db = db;
            _passwordService = passwordService;
        }

        public async Task Register(RegisterDto registerDto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            var existsingUser = await _db.Users.AnyAsync(u => u.Login == registerDto.Login);
            if (existsingUser)
            {
                throw new Exception("Юзер с таким логином уже существует");
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
                Password = _passwordService.HashPassword(registerDto.Password),
                GroupId = existingToken.GroupId
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task<int?> Login(LoginDto loginDto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == loginDto.Login);
            if (user == null) { return null; }
            var valid = _passwordService.VerifyPassword(loginDto.Password, user.Password);
            if (!valid) { return null; }
            return user.UserId;
        }
    }
}
