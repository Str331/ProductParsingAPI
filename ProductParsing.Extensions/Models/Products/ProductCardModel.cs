namespace ProductParsing.Extensions.Models.Products
{
    public class ProductCardModel
    {
        public required int ID { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public string? SourceUrl { get; init; }
        public string? SourceHost { get; init; }
        public bool HasImage { get; init; }
    }
}
