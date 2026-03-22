using Psychometric_Test_Designer.Core;
using Psychometric_Test_Designer.Data;
using Microsoft.EntityFrameworkCore;

namespace Psychometric_Test_Designer.Services
{
    public class TokenService
    {
        private AppDbContext _db;
        private TokenGenerator _tokenGenerator;
        public TokenService(AppDbContext db, TokenGenerator tokenGenerator)
        {
            _db = db;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<Models.Token> GenerateToken(string groupId)
        {
            var group = await _db.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId);
            if (group == null)
            {
                throw new Exception("Группа не найдена");
            }

            string token;
            do
            {
                token = _tokenGenerator.Generate();
            } while (await _db.Tokens.AnyAsync(t => t.TokenId == token));

            var tokenEntity = new Models.Token
            {
                TokenId = token,
                GroupId = groupId,
                NumberOfUses = group.StudentCount
            };

            _db.Tokens.Add(tokenEntity);
            await _db.SaveChangesAsync();

            return tokenEntity;
        }
    }
}
