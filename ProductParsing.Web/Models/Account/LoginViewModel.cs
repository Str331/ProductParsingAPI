using System.ComponentModel.DataAnnotations;

namespace ProductParsing.Web.Models.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Вкажіть логін.")]
        [StringLength(50, ErrorMessage = "Логін не може перевищувати 50 символів.")]
        [Display(Name = "Логін")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Вкажіть пароль.")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
