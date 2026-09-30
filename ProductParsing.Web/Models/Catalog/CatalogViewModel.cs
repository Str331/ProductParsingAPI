using ProductParsing.Extensions.Common;
using ProductParsing.Extensions.Models.Products;

namespace ProductParsing.Web.Models.Catalog
{
    public class CatalogViewModel
    {
        public required PagedResponse<ProductCardModel> Products { get; init; }
        public bool CanImport { get; init; }
        public bool CanManageProducts { get; init; }
        public required ImportFormModel ImportForm { get; init; }
        public ImportReportViewModel? ImportReport { get; init; }
        public string? ImportError { get; init; }

        public string ReturnUrl => Products.Page > 1 ? $"/?page={Products.Page}" : "/";
    }
}
