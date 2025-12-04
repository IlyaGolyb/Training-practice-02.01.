namespace ShoeStoreLLC.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; }
    }

    public static class UserRoles
    {
        public const int Admin = 1;
        public const int Manager = 2;
        public const int Client = 3;
    }
}