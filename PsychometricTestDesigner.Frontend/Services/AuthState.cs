namespace PsychometricTestDesigner.Frontend.Services;

public sealed class AuthState
{
    public event Action? Changed;

    public int UserId { get; private set; }
    public int GroupId { get; private set; }
    public string GroupName { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);
    public bool IsStudent => Role.Equals("Student", StringComparison.OrdinalIgnoreCase);
    public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    public bool IsPsychologist => Role.Equals("Psychologist", StringComparison.OrdinalIgnoreCase);
    public bool IsStaff => IsAdmin
        || IsPsychologist
        || Role.Equals("SocialTeacher", StringComparison.OrdinalIgnoreCase);
    public bool CanManageTests => IsAdmin || IsPsychologist;

    public string RoleLabel => Role switch
    {
        "Admin" => "Администратор",
        "Psychologist" => "Психолог",
        "SocialTeacher" => "Социальный педагог",
        "Student" => "Студент",
        _ => "Гость"
    };

    public string DisplayName => string.IsNullOrWhiteSpace(FullName)
        ? $"Пользователь {UserId}"
        : FullName;

    public string GroupLabel => string.IsNullOrWhiteSpace(GroupName)
        ? (GroupId > 0 ? GroupId.ToString() : "не указана")
        : GroupName;

    public string ProfileTitle => $"{DisplayName} - {RoleLabel}";
    public string ProfileSubtitle => $"Группа: {GroupLabel}";

    public void Set(int userId, int groupId, string groupName, string fullName, string role, string token)
    {
        UserId = userId;
        GroupId = groupId;
        GroupName = groupName;
        FullName = fullName;
        Role = role;
        Token = token;
        Changed?.Invoke();
    }

    public void Clear()
    {
        UserId = 0;
        GroupId = 0;
        GroupName = string.Empty;
        FullName = string.Empty;
        Role = string.Empty;
        Token = string.Empty;
        Changed?.Invoke();
    }
}

