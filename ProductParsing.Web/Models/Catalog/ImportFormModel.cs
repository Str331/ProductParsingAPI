using System.ComponentModel.DataAnnotations;

namespace ProductParsing.Web.Models.Catalog
{
    public class ImportFormModel
    {
        [Required(ErrorMessage = "Вставте посилання на сторінку з товарами.")]
        [StringLength(2048, ErrorMessage = "Посилання занадто довге.")]
        [Display(Name = "Посилання на сторінку з товарами")]
        public string ListingUrl { get; set; } = string.Empty;
    }
}
