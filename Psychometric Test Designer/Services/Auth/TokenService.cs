using Microsoft.EntityFrameworkCore;
using Psychometric_Test_Designer.Core;
using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.Models;

namespace Psychometric_Test_Designer.Services
{
    public class TokenService
    {
        private readonly AppDbContext _db;
        private readonly TokenGenerator _tokenGenerator;
        public TokenService(AppDbContext db, TokenGenerator tokenGenerator)
        {
            _db = db;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<Token> GenerateToken(int groupId)
        {
            var group = await _db.Groups.FindAsync(groupId);
            if (group == null)
            {
                throw new Exception("Группа не найдена");
            }

            try
            {
                string token;
                do
                {
                    token = _tokenGenerator.Generate();
                } while (await _db.Tokens.AnyAsync(t => t.TokenId == token));

                var tokenEntity = new Token
                {
                    TokenId = token,
                    GroupId = groupId,
                };

                _db.Tokens.Add(tokenEntity);
                await _db.SaveChangesAsync();

                return tokenEntity;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Ошибка при сохранении токена в базе данных: ", ex);
            }
        }

        public async Task<List<Token>> GetTokens()
        {
            var tokens = await _db.Tokens.Select(t => new Token
            {
                TokenId = t.TokenId,
                GroupId = t.GroupId,
                NumberOfUses = t.NumberOfUses
            }).ToListAsync();

            return tokens;
        }

        public async Task RemoveToken(string tokenId)
        {
            var token = await _db.Tokens.FindAsync(tokenId);
            if (token == null)
            {
                throw new Exception("Токен не найден");
            }
            _db.Tokens.Remove(token);
            await _db.SaveChangesAsync();
        }

        public async Task EditToken(string tokenId, int numberOfUses)
        {
            var token = await _db.Tokens.FindAsync(tokenId);
            if (token == null)
            {
                throw new Exception("Токен не найден");
            }
            token.NumberOfUses = numberOfUses;
            await _db.SaveChangesAsync();
        }
    }
}
