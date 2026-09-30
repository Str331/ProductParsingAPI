using AngleSharp.Dom;

namespace ProductParsing.Extensions.Scrapers.Parsing
{
    public record MicrodataProduct(string? Name, IElement? Description, string? Image, string? Url);

    public static class MicrodataReader
    {
        public static List<MicrodataProduct> FindProducts(IDocument document)
        {
            return document.QuerySelectorAll("[itemscope][itemtype]")
                           .Where(e => IsProductType(e.GetAttribute("itemtype")))
                           .Where(e => !HasProductAncestor(e))
                           .Select(Read)
                           .ToList();
        }

        private static MicrodataProduct Read(IElement item)
        {
            var name = Property(item, "name");
            var image = Property(item, "image");
            var url = Property(item, "url");

            return new MicrodataProduct(
                Name: name is null ? null : Value(name),
                Description: Property(item, "description"),
                Image: image is null ? null : Value(image),
                Url: url is not null ? Value(url) : item.QuerySelector("a[href]")?.GetAttribute("href"));
        }

        private static IElement? Property(IElement item, string name)
        {
            return item.QuerySelectorAll($"[itemprop~='{name}']").FirstOrDefault(e => NearestScope(e) == item);
        }

        private static string? Value(IElement element)
        {
            var value = element.LocalName switch
            {
                "meta" => element.GetAttribute("content"),
                "img" or "source" => element.GetAttribute("src") ?? element.GetAttribute("data-src"),
                "a" or "link" => element.GetAttribute("href"),
                _ => element.GetAttribute("content") ?? element.TextContent
            };

            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static IElement? NearestScope(IElement element)
        {
            var current = element.ParentElement;

            while (current is not null && !current.HasAttribute("itemscope"))
            {
                current = current.ParentElement;
            }

            return current;
        }

        private static bool HasProductAncestor(IElement element)
        {
            for (var current = element.ParentElement; current is not null; current = current.ParentElement)
            {
                if (current.HasAttribute("itemscope") && IsProductType(current.GetAttribute("itemtype")))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsProductType(string? itemType)
        {
            return itemType is not null
                && itemType.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                           .Any(t => t.EndsWith("schema.org/Product", StringComparison.OrdinalIgnoreCase));
        }
    }
}
