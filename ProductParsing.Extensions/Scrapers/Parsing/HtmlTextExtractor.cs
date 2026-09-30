using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ProductParsing.Extensions.Scrapers.Parsing
{
    public static partial class HtmlTextExtractor
    {
        private const string Ellipsis = "…";

        private static readonly HashSet<string> BlockElements = new(StringComparer.OrdinalIgnoreCase)
        {
            "p", "div", "li", "ul", "ol", "h1", "h2", "h3", "h4", "h5", "h6",
            "section", "article", "table", "tr", "blockquote", "header", "footer", "dl", "dt", "dd"
        };

        private static readonly HashSet<string> SkippedElements = new(StringComparer.OrdinalIgnoreCase)
        {
            "script", "style", "noscript", "template", "svg"
        };

        public static string? ToPlainText(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var decoded = WebUtility.HtmlDecode(value);
            var text = MarkupTagRegex().IsMatch(decoded) ? MarkupToText(decoded) : decoded;

            return Finish(text, maxLength);
        }

        public static string? FromElement(IElement? element, int maxLength)
        {
            if (element is null)
            {
                return null;
            }

            var builder = new StringBuilder();
            AppendChildren(element, builder);

            return Finish(builder.ToString(), maxLength);
        }

        private static string MarkupToText(string html)
        {
            using var document = new HtmlParser().ParseDocument($"<body>{html}</body>");
            var builder = new StringBuilder();

            if (document.Body is not null)
            {
                AppendChildren(document.Body, builder);
            }

            return builder.ToString();
        }

        private static void AppendChildren(INode node, StringBuilder builder)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType == NodeType.Text)
                {
                    builder.Append(AnyWhitespaceRegex().Replace(child.TextContent, " "));
                }
                else if (child is IElement element)
                {
                    AppendElement(element, builder);
                }
            }
        }

        private static void AppendElement(IElement element, StringBuilder builder)
        {
            if (SkippedElements.Contains(element.LocalName))
            {
                return;
            }

            if (element.LocalName.Equals("br", StringComparison.OrdinalIgnoreCase))
            {
                builder.Append('\n');
                return;
            }

            var isBlock = BlockElements.Contains(element.LocalName);

            if (isBlock)
            {
                builder.Append('\n');
            }

            AppendChildren(element, builder);

            if (isBlock)
            {
                builder.Append('\n');
            }
        }

        private static string? Finish(string text, int maxLength)
        {
            var normalized = NormalizeLines(text);

            return normalized.Length == 0 ? null : Truncate(normalized, maxLength);
        }

        private static string NormalizeLines(string text)
        {
            var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal)
                            .Replace('\r', '\n')
                            .Split('\n')
                            .Select(line => InlineWhitespaceRegex().Replace(line, " ").Trim());

            return ExcessBlankLinesRegex().Replace(string.Join('\n', lines), "\n\n").Trim();
        }

        private static string Truncate(string value, int maxLength)
        {
            if (value.Length <= maxLength)
            {
                return value;
            }

            var cut = value[..(maxLength - Ellipsis.Length)];
            var lastBreak = cut.LastIndexOfAny([' ', '\n']);

            if (lastBreak > maxLength / 2)
            {
                cut = cut[..lastBreak];
            }

            return cut.TrimEnd() + Ellipsis;
        }

        [GeneratedRegex(@"<\s*/?\s*[a-zA-Z][a-zA-Z0-9]*(\s[^<>]*)?/?\s*>")]
        private static partial Regex MarkupTagRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex AnyWhitespaceRegex();

        [GeneratedRegex(@"[^\S\n]+")]
        private static partial Regex InlineWhitespaceRegex();

        [GeneratedRegex(@"\n{3,}")]
        private static partial Regex ExcessBlankLinesRegex();
    }
}
