using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.CircuitBreaker;
using Polly.Timeout;
using ProductParsing.Extensions.Options;
using System.Net;

namespace ProductParsing.Extensions.Scrapers.Http
{
    public record FetchedContent(Uri FinalUrl, byte[] Content, string? MediaType, string? CharSet);

    public class HttpContentFetcher(IHttpClientFactory httpClientFactory, IOptions<ScrapingOptions> options, ILogger<HttpContentFetcher> logger)
    {
        public const string HttpClientName = "stores";
        private const int BufferSize = 81920;
        private const string TooLarge = "Відповідь магазину перевищує допустимий розмір.";

        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ScrapingOptions _options = options.Value;
        private readonly ILogger<HttpContentFetcher> _logger = logger;

        public async Task<FetchedContent> FetchAsync(Uri url, int maxBytes, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            var current = url;

            for (var redirects = 0; ; redirects++)
            {
                if (!PublicUrlPolicy.IsAllowed(current))
                {
                    throw new ScrapingException($"Адреса {current.Host} не дозволена: потрібна публічна http(s)-адреса без порту.");
                }

                using var request = new HttpRequestMessage(HttpMethod.Get, current);
                using var response = await SendAsync(client, request, ct);

                if (!IsRedirect(response.StatusCode))
                {
                    return await ReadAsync(current, response, maxBytes, ct);
                }

                if (redirects >= _options.MaxRedirects)
                {
                    throw new ScrapingException("Забагато перенаправлень.");
                }

                var location = response.Headers.Location ?? throw new ScrapingException("Магазин повернув перенаправлення без адреси.");
                current = location.IsAbsoluteUri ? location : new Uri(current, location);
            }
        }

        private async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct)
        {
            try
            {
                return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            }
            catch (HttpRequestException ex) when (ex.InnerException is ScrapingException blocked)
            {
                throw new ScrapingException(blocked.Reason, ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Request to {Url} failed", request.RequestUri);
                throw new ScrapingException("Не вдалося з'єднатися з магазином.", ex);
            }
            catch (TimeoutRejectedException ex)
            {
                throw new ScrapingException("Магазин не відповів вчасно.", ex);
            }
            catch (BrokenCircuitException ex)
            {
                throw new ScrapingException("Магазин тимчасово недоступний.", ex);
            }
        }

        private async Task<FetchedContent> ReadAsync(Uri url, HttpResponseMessage response, int maxBytes, CancellationToken ct)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw StatusError(response);
            }

            var content = response.Content;

            if (content.Headers.ContentLength > maxBytes)
            {
                throw new ScrapingException(TooLarge);
            }

            var bytes = await ReadLimitedAsync(content, maxBytes, ct);

            return new FetchedContent(url, bytes, content.Headers.ContentType?.MediaType, content.Headers.ContentType?.CharSet);
        }

        private async Task<byte[]> ReadLimitedAsync(HttpContent content, int maxBytes, CancellationToken ct)
        {
            try
            {
                await using var stream = await content.ReadAsStreamAsync(ct);
                using var buffer = new MemoryStream();
                var chunk = new byte[BufferSize];
                int read;

                while ((read = await stream.ReadAsync(chunk, ct)) > 0)
                {
                    if (buffer.Length + read > maxBytes)
                    {
                        throw new ScrapingException(TooLarge);
                    }

                    buffer.Write(chunk, 0, read);
                }

                return buffer.ToArray();
            }
            catch (Exception ex) when (ex is IOException or HttpRequestException or InvalidDataException)
            {
                _logger.LogWarning(ex, "Reading response body failed");

                throw new ScrapingException("Не вдалося отримати відповідь магазину.", ex);
            }
        }

        private static ScrapingException StatusError(HttpResponseMessage response)
        {
            var code = (int)response.StatusCode;

            var cloudflare = response.Headers.Contains("cf-mitigated") || response.Headers.Server.Any(s => string.Equals(s.Product?.Name, "cloudflare", StringComparison.OrdinalIgnoreCase));

            if (cloudflare && code is 403 or 503)
            {
                return new ScrapingException("Сайт блокує автоматичний доступ (захист Cloudflare), імпорт з нього неможливий.", statusCode: code);
            }

            if (code == 429)
            {
                return new ScrapingException("Сайт обмежив кількість запитів (HTTP 429), спробуйте пізніше.", statusCode: code);
            }

            return new ScrapingException($"Магазин повернув HTTP {code}.", statusCode: code);
        }

        private static bool IsRedirect(HttpStatusCode status)
        {
            return status is HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.SeeOther
                          or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;
        }
    }
}
