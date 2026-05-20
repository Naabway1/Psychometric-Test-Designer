namespace Psychometric_Test_Designer.Core
{
    public static class UserRoles
    {
        public const string Student = "Student";
        public const string Admin = "Admin";
        public const string SocialTeacher = "SocialTeacher";
        public const string Staff = Admin + "," + SocialTeacher;
    }
}
