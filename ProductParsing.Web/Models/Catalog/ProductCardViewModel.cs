using ProductParsing.Extensions.Models.Products;

namespace ProductParsing.Web.Models.Catalog
{
    public class ProductCardViewModel
    {
        public required ProductCardModel Product { get; init; }
        public bool CanManage { get; init; }
        public required string ReturnUrl { get; init; }
    }
}
