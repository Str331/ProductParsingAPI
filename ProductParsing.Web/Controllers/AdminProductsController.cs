using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductParsing.Extensions.Requests.Products;
using ProductParsing.Extensions.Services;
using ProductParsing.Web.Auth;
using ProductParsing.Web.Extensions;
using ProductParsing.Web.Models;
using ProductParsing.Web.Models.Admin;

namespace ProductParsing.Web.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.CanManageProducts), Route("admin/products")]
    public class AdminProductsController(IProductService service) : Controller
    {
        private const long MaxRequestBytes = 3 * 1024 * 1024;

        private readonly IProductService _service = service;

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            return View(await _service.GetAdminProducts(page));
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new ProductFormModel());
        }

        [HttpPost("create"), RequestSizeLimit(MaxRequestBytes), RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
        public async Task<IActionResult> Create(ProductFormModel form)
        {
            if (ModelState.IsValid == false)
            {
                return View(form);
            }

            var request = new CreateProductRequest
            {
                Name = form.Name,
                Description = form.Description,
                Image = await form.Image.ToBytes()
            };

            var response = await _service.CreateProduct(request);

            if (response.Success == false)
            {
                ModelState.AddError(response);

                return View(form);
            }

            TempData[FlashMessages.Key] = "Товар додано.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{ID:int}/edit")]
        public async Task<IActionResult> Edit(int ID, string? returnUrl)
        {
            var response = await _service.GetProductForEdit(ID);

            if (response.Success == false || response.Result is null)
            {
                return NotFound();
            }

            return View(ProductFormModel.FromModel(response.Result, Url.IsLocalUrl(returnUrl) ? returnUrl : null));
        }

        [HttpPost("{ID:int}/edit"), RequestSizeLimit(MaxRequestBytes), RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
        public async Task<IActionResult> Edit(int ID, ProductFormModel form)
        {
            if (ModelState.IsValid == false)
            {
                return await RenderEdit(ID, form);
            }

            var request = new UpdateProductRequest
            {
                ID = ID,
                Name = form.Name,
                Description = form.Description,
                ImageAction = form.ImageAction,
                Image = form.ImageAction == ImageAction.Replace ? await form.Image.ToBytes() : null
            };

            var response = await _service.UpdateProduct(request);

            if (response.Success == false)
            {
                if (response.ResponseCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }

                ModelState.AddError(response);
                return await RenderEdit(ID, form);
            }

            TempData[FlashMessages.Key] = "Зміни збережено.";

            return LocalRedirect(SafeReturnUrl(form.ReturnUrl));
        }

        [HttpPost("{ID:int}/delete")]
        public async Task<IActionResult> Delete(int ID, string? returnUrl)
        {
            var response = await _service.DeleteProduct(ID);

            TempData[FlashMessages.Key] = response.Success ? "Товар видалено." : response.Message;

            return LocalRedirect(SafeReturnUrl(returnUrl));
        }

        private async Task<IActionResult> RenderEdit(int ID, ProductFormModel form)
        {
            var response = await _service.GetProductForEdit(ID);

            if (response.Success == false || response.Result is null)
            {
                return NotFound();
            }

            form.Id = ID;
            form.ReturnUrl = Url.IsLocalUrl(form.ReturnUrl) ? form.ReturnUrl : null;
            form.SourceUrl = response.Result.SourceUrl;
            form.HasImage = response.Result.HasImage;

            ModelState.Remove(nameof(ProductFormModel.Id));
            ModelState.Remove(nameof(ProductFormModel.ReturnUrl));
            ModelState.Remove(nameof(ProductFormModel.SourceUrl));
            ModelState.Remove(nameof(ProductFormModel.HasImage));

            return View(form);
        }

        private string SafeReturnUrl(string? returnUrl)
        {
            return Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action(nameof(Index)) ?? "/admin/products";
        }
    }
}
