using AngleSharp.Dom;
using System.Text.Json;

namespace ProductParsing.Extensions.Scrapers.Parsing
{
    public record JsonLdProduct(string? Name, string? Description, List<string> Images, string? Url);

    public class JsonLdReader
    {
        private static readonly JsonDocumentOptions ParseOptions = new()
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        };

        private readonly List<JsonElement> _nodes;

        private JsonLdReader(List<JsonElement> nodes)
        {
            _nodes = nodes;
        }

        public static JsonLdReader FromDocument(IDocument document)
        {
            var nodes = new List<JsonElement>();

            foreach (var script in document.QuerySelectorAll("script[type='application/ld+json']"))
            {
                AddNodes(script.TextContent, nodes);
            }

            return new JsonLdReader(nodes);
        }

        public JsonLdProduct? FindProduct()
        {
            return FindProducts().FirstOrDefault();
        }

        public List<JsonLdProduct> FindProducts()
        {
            return _nodes.Where(n => HasType(n, "Product"))
                         .Select(n => new JsonLdProduct(GetString(n, "name"), GetString(n, "description"), GetImages(n), GetString(n, "url")))
                         .ToList();
        }

        public List<string> FindItemListUrls()
        {
            return _nodes.Where(n => HasType(n, "ItemList"))
                         .SelectMany(n => ArrayItems(n, "itemListElement"))
                         .Select(GetListItemUrl)
                         .OfType<string>()
                         .ToList();
        }

        private static void AddNodes(string json, List<JsonElement> nodes)
        {
            try
            {
                using var document = JsonDocument.Parse(json, ParseOptions);
                Flatten(document.RootElement, nodes);
            }
            catch (JsonException)
            {
            }
        }

        private static void Flatten(JsonElement element, List<JsonElement> nodes)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    Flatten(item, nodes);
                }
            }
            else if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("@graph", out var graph))
                {
                    Flatten(graph, nodes);
                }
                else
                {
                    nodes.Add(element.Clone());
                }
            }
        }

        private static bool HasType(JsonElement node, string type)
        {
            if (!node.TryGetProperty("@type", out var value))
            {
                return false;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => string.Equals(value.GetString(), type, StringComparison.OrdinalIgnoreCase),
                JsonValueKind.Array => value.EnumerateArray().Any(item => item.ValueKind == JsonValueKind.String
                                                                       && string.Equals(item.GetString(), type, StringComparison.OrdinalIgnoreCase)),
                _ => false
            };
        }

        private static string? GetString(JsonElement node, string property)
        {
            return node.ValueKind == JsonValueKind.Object && node.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        private static List<string> GetImages(JsonElement node)
        {
            if (!node.TryGetProperty("image", out var image))
            {
                return [];
            }

            var candidates = image.ValueKind == JsonValueKind.Array ? image.EnumerateArray().ToList() : [image];

            return candidates.Select(GetImageUrl).OfType<string>().Where(url => url.Length > 0).ToList();
        }

        private static string? GetImageUrl(JsonElement image)
        {
            return image.ValueKind switch
            {
                JsonValueKind.String => image.GetString(),
                JsonValueKind.Object => GetString(image, "url") ?? GetString(image, "contentUrl"),
                _ => null
            };
        }

        private static List<JsonElement> ArrayItems(JsonElement node, string property)
        {
            return node.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Array
                ? value.EnumerateArray().ToList()
                : [];
        }

        private static string? GetListItemUrl(JsonElement listItem)
        {
            if (listItem.ValueKind == JsonValueKind.String)
            {
                return listItem.GetString();
            }

            if (listItem.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (GetString(listItem, "url") is { } url)
            {
                return url;
            }

            if (!listItem.TryGetProperty("item", out var item))
            {
                return null;
            }

            return item.ValueKind == JsonValueKind.String
                ? item.GetString()
                : GetString(item, "url") ?? GetString(item, "@id");
        }
    }
}
