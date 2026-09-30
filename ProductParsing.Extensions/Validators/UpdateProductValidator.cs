using FluentValidation;
using ProductParsing.Extensions.Requests.Products;

namespace ProductParsing.Extensions.Validators
{
    public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
    {
        public const string ImageRequiredMessage = "Оберіть файл зображення для заміни.";

        public UpdateProductValidator()
        {
            Include(new CreateProductValidator());

            RuleFor(x => x.ImageAction)
                .IsInEnum();

            RuleFor(x => x.Image)
                .NotEmpty()
                .When(x => x.ImageAction == ImageAction.Replace)
                .WithMessage(ImageRequiredMessage);
        }
    }
}
