using Microsoft.Extensions.Options;
using ProductParsing.Extensions.Helpers;
using ProductParsing.Extensions.Options;

namespace ProductParsing.Extensions.Scrapers.Http
{
    public record DownloadedImage(byte[] Content, string ContentType, Uri SourceUrl);

    public interface IImageDownloader
    {
        Task<DownloadedImage> DownloadAsync(Uri imageUrl, CancellationToken ct);
    }

    public class HttpImageDownloader(HttpContentFetcher fetcher, IOptions<ScrapingOptions> options) : IImageDownloader
    {
        private readonly HttpContentFetcher _fetcher = fetcher;
        private readonly ScrapingOptions _options = options.Value;

        public async Task<DownloadedImage> DownloadAsync(Uri imageUrl, CancellationToken ct)
        {
            var content = await _fetcher.FetchAsync(imageUrl, _options.MaxImageBytes, ct);
            var contentType = ImageContentInspector.DetectContentType(content.Content)
                ?? throw new ScrapingException("Файл за посиланням не є зображенням JPEG, PNG, WEBP або GIF.");

            return new DownloadedImage(content.Content, contentType, imageUrl);
        }
    }
}
