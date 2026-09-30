using ProductParsing.Extensions.Data;

namespace ProductParsing.Extensions.Extensions
{
    public static class ProductExtensions
    {
        public static void SetImage(this Product product, byte[] content, string contentType, string? sourceUrl = null)
        {
            product.Image ??= new ProductImage();

            product.Image.Content = content;
            product.Image.ContentType = contentType;
            product.Image.SourceUrl = sourceUrl;
        }

        public static string? NormalizeDescription(this string? description)
        {
            return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }
    }
}
