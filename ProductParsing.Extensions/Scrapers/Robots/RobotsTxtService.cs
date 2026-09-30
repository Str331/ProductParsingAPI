using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductParsing.Extensions.Options;
using ProductParsing.Extensions.Scrapers.Http;
using System.Collections.Concurrent;
using System.Text;

namespace ProductParsing.Extensions.Scrapers.Robots
{
    public interface IRobotsTxtService
    {
        Task<bool> IsAllowedAsync(Uri url, CancellationToken ct);
    }

    public class RobotsTxtService(HttpContentFetcher fetcher, IOptions<ScrapingOptions> options, ILogger<RobotsTxtService> logger) : IRobotsTxtService
    {
        private const int MaxBytes = 500 * 1024;

        private readonly HttpContentFetcher _fetcher = fetcher;
        private readonly ScrapingOptions _options = options.Value;
        private readonly ILogger<RobotsTxtService> _logger = logger;
        private readonly ConcurrentDictionary<string, Lazy<Task<RobotsTxt>>> _cache = new(StringComparer.OrdinalIgnoreCase);

        public async Task<bool> IsAllowedAsync(Uri url, CancellationToken ct)
        {
            var origin = url.GetLeftPart(UriPartial.Authority);
            var robots = await _cache.GetOrAdd(origin, key => new Lazy<Task<RobotsTxt>>(() => Load(new Uri(key + "/robots.txt"), ct))).Value;

            return robots.IsAllowed(url);
        }

        private async Task<RobotsTxt> Load(Uri robotsUrl, CancellationToken ct)
        {
            try
            {
                var content = await _fetcher.FetchAsync(robotsUrl, MaxBytes, ct);

                return RobotsTxt.Parse(Encoding.UTF8.GetString(content.Content), ProductToken());
            }
            catch (ScrapingException ex) when (ex.StatusCode is >= 400 and < 500)
            {
                return RobotsTxt.AllowAll;
            }
            catch (ScrapingException ex)
            {
                _logger.LogWarning("robots.txt {RobotsUrl} is unavailable: {Reason}", robotsUrl, ex.Reason);
                return RobotsTxt.DisallowAll;
            }
        }

        private string ProductToken()
        {
            return _options.UserAgent.Split('/', ' ')[0];
        }
    }
}
