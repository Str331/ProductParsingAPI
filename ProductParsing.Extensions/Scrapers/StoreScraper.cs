using ProductParsing.Extensions.Data;
using ProductParsing.Extensions.Scrapers.Http;
using ProductParsing.Extensions.Scrapers.Parsing;

namespace ProductParsing.Extensions.Scrapers
{
    public interface IStoreScraper
    {
        Task<IReadOnlyList<Uri>> GetProductLinksAsync(Uri pageUrl, CancellationToken ct);

        Task<ScrapedProduct> ScrapeProductAsync(Uri productUrl, CancellationToken ct);
    }

    public class StoreScraper(IHtmlDocumentLoader loader) : IStoreScraper
    {
        private readonly IHtmlDocumentLoader _loader = loader;

        public async Task<IReadOnlyList<Uri>> GetProductLinksAsync(Uri pageUrl, CancellationToken ct)
        {
            using var page = await _loader.LoadAsync(pageUrl, ct);

            var links = ProductLinkFinder.Find(page.Document, page.Url);

            if (links.Count == 0)
            {
                throw new ScrapingException("На сторінці не знайдено товарів.");
            }

            return links;
        }

        public async Task<ScrapedProduct> ScrapeProductAsync(Uri productUrl, CancellationToken ct)
        {
            using var page = await _loader.LoadAsync(productUrl, ct);

            var extracted = ProductPageReader.Read(page.Document);
            var name = HtmlTextExtractor.ToPlainText(extracted.Name, Product.NameMaxLength)
                ?? throw new ScrapingException("Не знайдено назву товару.");

            return new ScrapedProduct(productUrl, name, extracted.Description, ResolveImageUrl(extracted.ImageUrl, page.Url));
        }

        private static Uri? ResolveImageUrl(string? imageUrl, Uri pageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) || !Uri.TryCreate(pageUrl, imageUrl.Trim(), out var absolute))
            {
                return null;
            }

            return PublicUrlPolicy.IsAllowed(absolute) ? absolute : null;
        }
    }
}
