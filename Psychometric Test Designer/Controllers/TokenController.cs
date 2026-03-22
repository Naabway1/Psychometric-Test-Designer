using Microsoft.AspNetCore.Mvc;
using Psychometric_Test_Designer.Data;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.Services;
using Microsoft.EntityFrameworkCore;

namespace Psychometric_Test_Designer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private TokenService _tokenService;
        private readonly AppDbContext _db;
        public TokenController(TokenService tokenService, AppDbContext db)
        {
            _tokenService = tokenService;
            _db = db;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> CreateToken([FromBody] CreateTokenDto dto)
        {
            try
            {
                var token = await _tokenService.GenerateToken(dto.GroupId);
                return Ok(new TokenResponseDto
                {
                    TokenId = token.TokenId,
                    GroupId = token.GroupId,
                    NumberOfUses = token.NumberOfUses
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<List<TokenResponseDto>> GetTokens()
        {
            var tokens = await _db.Tokens.Select(t => new TokenResponseDto
            {
                TokenId = t.TokenId,
                GroupId = t.GroupId,
                NumberOfUses = t.NumberOfUses
            }).ToListAsync();

            return tokens;
        }
    }
}
