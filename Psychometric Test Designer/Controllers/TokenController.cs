using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Psychometric_Test_Designer.DTOs;
using Psychometric_Test_Designer.DTOs.Auth;
using Psychometric_Test_Designer.Services;

namespace Psychometric_Test_Designer.Controllers
{
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    [Route("api/tokens")]
    public class TokenController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public TokenController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateToken([FromBody] CreateTokenDto dto)
        {
            var token = await _tokenService.GenerateToken(dto.GroupId, dto.NumberOfUses);

            return Ok(new TokenResponseDto
            {
                TokenId = token.TokenId,
                GroupId = token.GroupId,
                NumberOfUses = token.NumberOfUses
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTokens()
        {
            var tokens = await _tokenService.GetTokens();

            var result = tokens.Select(t => new TokenResponseDto
            {
                TokenId = t.TokenId,
                GroupId = t.GroupId,
                NumberOfUses = t.NumberOfUses
            }).ToList();

            return Ok(result);
        }

        [HttpDelete("{tokenId}")]
        public async Task<IActionResult> RemoveToken([FromRoute] string tokenId)
        {
            await _tokenService.RemoveToken(tokenId);
            return Ok();
        }

        [HttpPatch("{tokenId}")]
        public async Task<IActionResult> EditToken([FromRoute] string tokenId, [FromBody] EditTokenDto dto)
        {
            await _tokenService.EditToken(tokenId, dto.NumberOfUses);
            return Ok();
        }
    }
}
