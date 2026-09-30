using ProductParsing.Extensions.Data;
using ProductParsing.Extensions.Models.Products;
using ProductParsing.Extensions.Requests.Products;
using ProductParsing.Web.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ProductParsing.Web.Models.Admin
{
    public class ProductFormModel
    {
        public const int _maxImageBytes = 2 * 1024 * 1024;

        public int? Id { get; set; }

        [Required(ErrorMessage = "Вкажіть назву товару.")]
        [StringLength(Product.NameMaxLength, ErrorMessage = "Назва не може перевищувати 300 символів.")]
        [Display(Name = "Назва")]
        public string Name { get; set; } = string.Empty;

        [StringLength(Product.DescriptionMaxLength, ErrorMessage = "Опис не може перевищувати 8000 символів.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Опис")]
        public string? Description { get; set; }

        [MaxFileSize(_maxImageBytes, ErrorMessage = "Зображення не може перевищувати 2 МБ.")]
        [Display(Name = "Файл зображення")]
        public IFormFile? Image { get; set; }

        public ImageAction ImageAction { get; set; } = ImageAction.Keep;

        public bool HasImage { get; set; }

        public string? SourceUrl { get; set; }

        public string? ReturnUrl { get; set; }

        public static ProductFormModel FromModel(ProductEditModel product, string? localReturnUrl)
        {
            return new ProductFormModel
            {
                Id = product.ID,
                Name = product.Name,
                Description = product.Description,
                HasImage = product.HasImage,
                SourceUrl = product.SourceUrl,
                ReturnUrl = localReturnUrl
            };
        }
    }
}
