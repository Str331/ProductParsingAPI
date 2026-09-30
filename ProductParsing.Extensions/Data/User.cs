namespace ProductParsing.Extensions.Data
{
    public class User
    {
        public const int UserNameMaxLength = 50;

        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public int RoleId { get; set; }

        public virtual Role Role { get; set; } = null!;
    }
}
