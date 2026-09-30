using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Options;
using ProductParsing.Extensions.Options;
using System.Text;

namespace ProductParsing.Extensions.Scrapers.Http
{
    public interface IHtmlDocumentLoader
    {
        Task<HtmlPage> LoadAsync(Uri url, CancellationToken ct);
    }

    public class HtmlPage(Uri url, IDocument document) : IDisposable
    {
        public Uri Url { get; } = url;
        public IDocument Document { get; } = document;

        public void Dispose()
        {
            Document.Dispose();
        }
    }

    public class HtmlDocumentLoader(HttpContentFetcher fetcher, IOptions<ScrapingOptions> options) : IHtmlDocumentLoader
    {
        private readonly HttpContentFetcher _fetcher = fetcher;
        private readonly ScrapingOptions _options = options.Value;

        public async Task<HtmlPage> LoadAsync(Uri url, CancellationToken ct)
        {
            var content = await _fetcher.FetchAsync(url, _options.MaxHtmlBytes, ct);
            var html = GetEncoding(content.CharSet).GetString(content.Content);

            return new HtmlPage(content.FinalUrl, new HtmlParser().ParseDocument(html));
        }

        private static Encoding GetEncoding(string? charset)
        {
            if (string.IsNullOrWhiteSpace(charset))
            {
                return Encoding.UTF8;
            }

            try
            {
                return Encoding.GetEncoding(charset.Trim('"', '\''));
            }
            catch (ArgumentException)
            {
                return Encoding.UTF8;
            }
        }
    }
}
