namespace ProductParsing.Extensions.Data
{
    public class Role
    {
        public const int NameMaxLength = 50;

        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
