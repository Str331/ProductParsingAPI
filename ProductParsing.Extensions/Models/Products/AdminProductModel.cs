namespace ProductParsing.Extensions.Models.Products
{
    public class AdminProductModel
    {
        public required int ID { get; init; }
        public required string Name { get; init; }
        public string? SourceHost { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public bool HasImage { get; init; }
    }
}
