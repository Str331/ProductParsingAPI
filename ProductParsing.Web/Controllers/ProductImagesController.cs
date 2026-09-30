using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductParsing.Extensions.Services;

namespace ProductParsing.Web.Controllers
{
    [AllowAnonymous]
    public class ProductImagesController(IProductService service) : Controller
    {
        private readonly IProductService _service = service;

        [HttpGet("/products/{ID:int}/image")]
        public async Task<IActionResult> Get(int ID)
        {
            var response = await _service.GetProductImage(ID);

            if (response.Success == false || response.Result is null)
            {
                return NotFound();
            }

            return File(response.Result.Content, response.Result.ContentType);
        }
    }
}
