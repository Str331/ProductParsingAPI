namespace ProductParsing.Extensions.Data
{
    public class ProductImage
    {
        public const int MaxBytes = 5 * 1024 * 1024;
        public const int SourceUrlMaxLength = 2048;

        public int ProductId { get; set; }
        public byte[] Content { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public string? SourceUrl { get; set; }
    }
}
