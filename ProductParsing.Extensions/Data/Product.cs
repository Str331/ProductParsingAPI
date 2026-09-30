namespace ProductParsing.Extensions.Data
{
    public class Product
    {
        public const int NameMaxLength = 300;
        public const int DescriptionMaxLength = 8000;
        public const int SourceUrlMaxLength = 800;

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? SourceUrl { get; set; }
        public string? SourceHost { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public virtual ProductImage? Image { get; set; }
    }
}
