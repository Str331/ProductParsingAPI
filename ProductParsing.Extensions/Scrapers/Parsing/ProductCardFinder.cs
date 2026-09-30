using AngleSharp.Dom;
using System.Text.RegularExpressions;

namespace ProductParsing.Extensions.Scrapers.Parsing
{
    public static partial class ProductCardFinder
    {
        private const int MinCards = 3;
        private const int MaxDepth = 8;

        public static List<Uri> Find(IDocument document, Uri pageUrl)
        {
            var links = document.QuerySelectorAll("a[href]")
                                .Select(a => (Anchor: a, Url: ProductUrlNormalizer.TryNormalize(a.GetAttribute("href"), pageUrl, out var url) ? url : null))
                                .Where(x => x.Url is not null && IsCandidate(x.Url, pageUrl))
                                .Select(x => (x.Anchor, Url: x.Url!))
                                .ToList();

            var withPrice = FindInGroups(links, requirePrice: true);

            return withPrice.Count > 0 ? withPrice : FindInGroups(links, requirePrice: false);
        }

        private static List<Uri> FindInGroups(List<(IElement Anchor, Uri Url)> links, bool requirePrice)
        {
            var cache = new Dictionary<IElement, bool>();
            var cards = new Dictionary<IElement, List<Uri>>();

            foreach (var (anchor, url) in links)
            {
                var card = FindCard(anchor, requirePrice, cache);

                if (card is null)
                {
                    continue;
                }

                if (!cards.TryGetValue(card, out var urls))
                {
                    cards[card] = urls = [];
                }

                urls.Add(url);
            }

            var best = cards.GroupBy(c => Signature(c.Key))
                            .Where(g => g.Count() >= MinCards)
                            .Select(g => g.Select(c => MostFrequent(c.Value)).DistinctBy(u => u.AbsoluteUri, StringComparer.OrdinalIgnoreCase).ToList())
                            .OrderByDescending(urls => urls.Count)
                            .FirstOrDefault();

            return best ?? [];
        }

        private static IElement? FindCard(IElement anchor, bool requirePrice, Dictionary<IElement, bool> cache)
        {
            var current = anchor;

            for (var depth = 0; current is not null && depth <= MaxDepth; depth++, current = current.ParentElement)
            {
                if (current.LocalName is "body" or "html")
                {
                    return null;
                }

                if (!cache.TryGetValue(current, out var isCard))
                {
                    isCard = current.QuerySelector("img, picture") is not null && (!requirePrice || PriceRegex().IsMatch(current.TextContent));
                    cache[current] = isCard;
                }

                if (isCard)
                {
                    return current;
                }
            }

            return null;
        }

        private static Uri MostFrequent(List<Uri> urls)
        {
            return urls.GroupBy(u => u.AbsoluteUri, StringComparer.OrdinalIgnoreCase)
                       .OrderByDescending(g => g.Count())
                       .First()
                       .First();
        }

        private static string Signature(IElement card)
        {
            var classes = card.ClassList.Where(c => !c.Any(char.IsDigit)).Order(StringComparer.Ordinal);

            return card.LocalName + "." + string.Join('.', classes);
        }

        private static bool IsCandidate(Uri url, Uri pageUrl)
        {
            return ProductUrlNormalizer.IsSameSite(url, pageUrl)
                && url.AbsolutePath != "/"
                && !string.Equals(url.PathAndQuery, pageUrl.PathAndQuery, StringComparison.OrdinalIgnoreCase);
        }

        [GeneratedRegex(@"\d[\d\s.,']*\s*(₴|грн|uah|\$|€|£|zł|pln|kč|czk|lei|ron|usd|eur)|(\$|€|£)\s*\d", RegexOptions.IgnoreCase)]
        private static partial Regex PriceRegex();
    }
}
