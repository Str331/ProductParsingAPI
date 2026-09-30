using AngleSharp.Dom;
using ProductParsing.Extensions.Data;

namespace ProductParsing.Extensions.Scrapers.Parsing
{
    public record ExtractedProduct(string? Name, string? Description, string? ImageUrl);

    public static class ProductPageReader
    {
        private static readonly HashSet<string> SkippedContainers = new(StringComparer.OrdinalIgnoreCase) { "header", "footer", "nav", "aside", "a" };

        public static ExtractedProduct Read(IDocument document)
        {
            var jsonLd = JsonLdReader.FromDocument(document).FindProduct();
            var microdata = MicrodataReader.FindProducts(document).FirstOrDefault();

            var name = FirstNotBlank(jsonLd?.Name, microdata?.Name, document.QuerySelector("h1")?.TextContent, Meta(document, "og:title"), document.Title);

            var image = FirstNotBlank(jsonLd?.Images.FirstOrDefault(),
                                      microdata?.Image,
                                      Meta(document, "og:image"),
                                      document.QuerySelector("link[rel='image_src']")?.GetAttribute("href"));

            return new ExtractedProduct(name, Description(document, jsonLd, microdata), image);
        }

        private static string? Description(IDocument document, JsonLdProduct? jsonLd, MicrodataProduct? microdata)
        {
            var candidates = new List<string?>
            {
                HtmlTextExtractor.ToPlainText(jsonLd?.Description, Product.DescriptionMaxLength),
                HtmlTextExtractor.FromElement(microdata?.Description, Product.DescriptionMaxLength),
                HtmlTextExtractor.ToPlainText(Meta(document, "og:description"), Product.DescriptionMaxLength),
                HtmlTextExtractor.ToPlainText(document.QuerySelector("meta[name='description']")?.GetAttribute("content"), Product.DescriptionMaxLength)
            };

            candidates.AddRange(DescriptionBlocks(document).Select(e => HtmlTextExtractor.FromElement(e, Product.DescriptionMaxLength)));

            return candidates.OfType<string>().MaxBy(text => text.Length);
        }

        private static IEnumerable<IElement> DescriptionBlocks(IDocument document)
        {
            return document.Body?.Descendants<IElement>()
                                 .Where(e => IsDescriptionLike(e.Id) || e.ClassList.Any(IsDescriptionLike))
                                 .Where(e => !e.Ancestors<IElement>().Any(a => SkippedContainers.Contains(a.LocalName)))
                   ?? [];
        }

        private static bool IsDescriptionLike(string? value)
        {
            return value is not null && value.Contains("description", StringComparison.OrdinalIgnoreCase);
        }

        private static string? FirstNotBlank(params string?[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        }

        private static string? Meta(IDocument document, string property)
        {
            var value = document.QuerySelector($"meta[property='{property}']")?.GetAttribute("content");

            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
