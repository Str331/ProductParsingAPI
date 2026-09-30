using FluentValidation;
using ProductParsing.Extensions.Data;
using ProductParsing.Extensions.Helpers;
using ProductParsing.Extensions.Requests.Products;

namespace ProductParsing.Extensions.Validators
{
    public class CreateProductValidator : AbstractValidator<CreateProductRequest>
    {
        public const string InvalidImageMessage = "Файл не є зображенням JPEG, PNG, WEBP або GIF.";

        public CreateProductValidator()
        {
            RuleFor(x => x.Name)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Назва обов'язкова.");

            RuleFor(x => x.Name)
                .Must(name => name.Trim().Length <= Product.NameMaxLength)
                .When(x => x.Name is not null)
                .WithMessage($"Назва не може перевищувати {Product.NameMaxLength} символів.");

            RuleFor(x => x.Description)
                .Must(description => description!.Trim().Length <= Product.DescriptionMaxLength)
                .When(x => x.Description is not null)
                .WithMessage($"Опис не може перевищувати {Product.DescriptionMaxLength} символів.");

            RuleFor(x => x.Image)
                .Must(image => image!.Length <= ProductImage.MaxBytes)
                .WithMessage("Зображення перевищує 5 МБ.")
                .Must(image => ImageContentInspector.DetectContentType(image) is not null)
                .WithMessage(InvalidImageMessage)
                .When(x => x.Image is { Length: > 0 });
        }
    }
}
