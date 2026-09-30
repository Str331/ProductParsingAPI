using ProductParsing.Extensions.Data;
using System.Diagnostics.CodeAnalysis;

namespace ProductParsing.Extensions.Scrapers.Parsing
{
    public static class ProductUrlNormalizer
    {
        private const string WwwPrefix = "www.";

        private static readonly string[] TrackingParams = ["gclid", "fbclid", "yclid", "msclkid", "_ga", "_gl"];

        public static bool TryNormalize(string? href, Uri baseUrl, [NotNullWhen(true)] out Uri? normalized)
        {
            normalized = null;

            if (string.IsNullOrWhiteSpace(href) || !Uri.TryCreate(baseUrl, href.Trim(), out var absolute))
            {
                return false;
            }

            if (absolute.Scheme != Uri.UriSchemeHttp && absolute.Scheme != Uri.UriSchemeHttps)
            {
                return false;
            }

            var candidate = new UriBuilder(absolute.Scheme, absolute.Host, absolute.IsDefaultPort ? -1 : absolute.Port)
            {
                Path = absolute.AbsolutePath,
                Query = CleanQuery(absolute.Query)
            }.Uri;

            if (candidate.AbsoluteUri.Length > Product.SourceUrlMaxLength)
            {
                return false;
            }

            normalized = candidate;
            return true;
        }

        public static bool IsSameSite(Uri url, Uri other)
        {
            return string.Equals(StripWww(url.Host), StripWww(other.Host), StringComparison.OrdinalIgnoreCase);
        }

        private static string CleanQuery(string query)
        {
            var parts = query.TrimStart('?')
                             .Split('&', StringSplitOptions.RemoveEmptyEntries)
                             .Where(p => !IsTracking(p.Split('=')[0]));

            return string.Join('&', parts);
        }

        private static bool IsTracking(string name)
        {
            return name.StartsWith("utm_", StringComparison.OrdinalIgnoreCase) || TrackingParams.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        private static string StripWww(string host)
        {
            return host.StartsWith(WwwPrefix, StringComparison.OrdinalIgnoreCase) ? host[WwwPrefix.Length..] : host;
        }
    }
}
