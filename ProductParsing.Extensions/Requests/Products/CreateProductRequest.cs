namespace ProductParsing.Extensions.Requests.Products
{
    public class CreateProductRequest
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public byte[]? Image { get; init; }
    }
}
