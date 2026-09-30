using System.Globalization;

namespace ProductParsing.Web.Models
{
    public class PaginationViewModel
    {
        public int Page { get; init; }
        public int TotalPages { get; init; }
        public required string BasePath { get; init; }

        public string UrlFor(int page)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{BasePath}?page={Math.Clamp(page, 1, Math.Max(1, TotalPages))}");
        }
    }
}
