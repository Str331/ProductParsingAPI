namespace ProductParsing.Extensions.Models.Products
{
    public class ProductImageModel
    {
        public required byte[] Content { get; init; }
        public required string ContentType { get; init; }
    }
}
