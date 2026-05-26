namespace Psychometric_Test_Designer.DTOs.Auth
{
    public class RegisterStaffDto
    {
        public string Login { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public int GroupId { get; set; }
        public string Role { get; set; } = "Admin";
        public string BootstrapKey { get; set; }
    }
}
