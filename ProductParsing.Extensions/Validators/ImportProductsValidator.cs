using FluentValidation;
using ProductParsing.Extensions.Requests.Import;

namespace ProductParsing.Extensions.Validators
{
    public class ImportProductsValidator : AbstractValidator<ImportProductsRequest>
    {
        public const string InvalidUrlMessage = "Вкажіть повну адресу сторінки каталогу, що починається з http:// або https://.";

        public ImportProductsValidator()
        {
            RuleFor(x => x.ListingUrl)
                .Must(BeHttpUrl)
                .WithMessage(InvalidUrlMessage);
        }

        private static bool BeHttpUrl(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || !Uri.TryCreate(value.Trim(), UriKind.Absolute, out var url))
            {
                return false;
            }

            return url.Scheme == Uri.UriSchemeHttps || url.Scheme == Uri.UriSchemeHttp;
        }
    }
}
