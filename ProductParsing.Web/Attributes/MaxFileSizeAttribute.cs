using System.ComponentModel.DataAnnotations;

namespace ProductParsing.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MaxFileSizeAttribute(long maxBytes) : ValidationAttribute
    {
        public long MaxBytes { get; } = maxBytes;

        public override bool IsValid(object? value)
        {
            return value is not IFormFile file || file.Length <= MaxBytes;
        }
    }
}
