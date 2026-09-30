using Microsoft.AspNetCore.Mvc.ModelBinding;
using ProductParsing.Extensions.Common;

namespace ProductParsing.Web.Extensions
{
    public static class ModelStateExtensions
    {
        public static void AddError(this ModelStateDictionary modelState, ServiceResponse response)
        {
            modelState.AddModelError(response.Field ?? string.Empty, response.Message ?? string.Empty);
        }

        public static string? FirstError(this ModelStateDictionary modelState)
        {
            return modelState.Values.SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .FirstOrDefault(m => m.Length > 0);
        }
    }
}
