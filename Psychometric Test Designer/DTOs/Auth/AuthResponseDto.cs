namespace Psychometric_Test_Designer.DTOs.Auth
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
