namespace ProductParsing.Extensions.Requests.Products
{
    public class UpdateProductRequest : CreateProductRequest
    {
        public required int ID { get; init; }
        public ImageAction ImageAction { get; init; }
    }
}
