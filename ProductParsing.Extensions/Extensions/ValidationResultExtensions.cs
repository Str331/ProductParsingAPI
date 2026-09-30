using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using ProductParsing.Extensions.Common;

namespace ProductParsing.Extensions.Extensions
{
    public static class ValidationResultExtensions
    {
        public static ServiceResponse<T> ToResponse<T>(this ValidationResult result)
        {
            var error = result.Errors.First();

            return new() { Success = false, Message = error.ErrorMessage, Field = error.PropertyName, ResponseCode = StatusCodes.Status400BadRequest };
        }

        public static ServiceResponse ToResponse(this ValidationResult result)
        {
            return result.ToResponse<object>();
        }
    }
}
