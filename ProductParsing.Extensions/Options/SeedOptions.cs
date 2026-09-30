namespace ProductParsing.Extensions.Options
{
    public class SeedOptions
    {
        public List<SeedUserOptions> Users { get; set; } = [];
    }

    public class SeedUserOptions
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
