namespace ProductParsing.Extensions.Scrapers.Http
{
    public static class PublicUrlPolicy
    {
        private static readonly string[] LocalSuffixes = [".localhost", ".local", ".internal", ".lan", ".home.arpa"];

        public static bool IsAllowed(Uri url)
        {
            if (!url.IsAbsoluteUri || (url.Scheme != Uri.UriSchemeHttps && url.Scheme != Uri.UriSchemeHttp) || !url.IsDefaultPort)
            {
                return false;
            }

            if (url.HostNameType != UriHostNameType.Dns || !url.Host.Contains('.'))
            {
                return false;
            }

            return !LocalSuffixes.Any(suffix => url.Host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }
    }
}
