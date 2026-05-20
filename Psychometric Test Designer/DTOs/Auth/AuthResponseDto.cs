namespace Psychometric_Test_Designer.DTOs.Auth
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
