namespace ProductParsing.Extensions.Scrapers
{
    public record ScrapedProduct(Uri SourceUrl, string Name, string? Description, Uri? ImageUrl);
}
