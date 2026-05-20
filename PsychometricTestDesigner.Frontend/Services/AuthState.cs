namespace PsychometricTestDesigner.Frontend.Services;

public sealed class AuthState
{
    public event Action? Changed;

    public int UserId { get; private set; }
    public int GroupId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);
    public bool IsStudent => Role.Equals("Student", StringComparison.OrdinalIgnoreCase);
    public bool IsStaff => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
        || Role.Equals("SocialTeacher", StringComparison.OrdinalIgnoreCase);

    public string RoleLabel => Role switch
    {
        "Admin" => "Администратор",
        "SocialTeacher" => "Социальный педагог",
        "Student" => "Студент",
        _ => "Гость"
    };

    public void Set(int userId, int groupId, string role, string token)
    {
        UserId = userId;
        GroupId = groupId;
        Role = role;
        Token = token;
        Changed?.Invoke();
    }

    public void Clear()
    {
        UserId = 0;
        GroupId = 0;
        Role = string.Empty;
        Token = string.Empty;
        Changed?.Invoke();
    }
}

