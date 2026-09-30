using AngleSharp.Dom;

namespace ProductParsing.Extensions.Scrapers.Parsing
{
    public static class ProductLinkFinder
    {
        public static List<Uri> Find(IDocument document, Uri pageUrl)
        {
            var jsonLd = JsonLdReader.FromDocument(document);

            var fromItemList = Normalize(jsonLd.FindItemListUrls(), pageUrl);

            if (fromItemList.Count > 0)
            {
                return fromItemList;
            }

            var jsonLdProducts = jsonLd.FindProducts();
            var fromJsonLd = Normalize(jsonLdProducts.Select(p => p.Url).OfType<string>(), pageUrl);

            if (fromJsonLd.Count >= 2)
            {
                return fromJsonLd;
            }

            if (jsonLdProducts.Count == 1 || IsOpenGraphProduct(document))
            {
                return Normalize([pageUrl.AbsoluteUri], pageUrl);
            }

            var microdata = MicrodataReader.FindProducts(document);
            var fromMicrodata = Normalize(microdata.Select(p => p.Url).OfType<string>(), pageUrl);

            if (fromMicrodata.Count >= 2)
            {
                return fromMicrodata;
            }

            if (microdata.Count == 1)
            {
                return Normalize([pageUrl.AbsoluteUri], pageUrl);
            }

            return ProductCardFinder.Find(document, pageUrl);
        }

        private static bool IsOpenGraphProduct(IDocument document)
        {
            var type = document.QuerySelector("meta[property='og:type']")?.GetAttribute("content");

            return type is not null && type.Contains("product", StringComparison.OrdinalIgnoreCase);
        }

        private static List<Uri> Normalize(IEnumerable<string> hrefs, Uri pageUrl)
        {
            return hrefs.Select(href => ProductUrlNormalizer.TryNormalize(href, pageUrl, out var url) ? url : null)
                        .OfType<Uri>()
                        .Where(url => ProductUrlNormalizer.IsSameSite(url, pageUrl))
                        .DistinctBy(url => url.AbsoluteUri, StringComparer.OrdinalIgnoreCase)
                        .ToList();
        }
    }
}
