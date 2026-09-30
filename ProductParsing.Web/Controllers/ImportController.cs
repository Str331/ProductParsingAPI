using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductParsing.Extensions.Requests.Import;
using ProductParsing.Extensions.Services;
using ProductParsing.Extensions.Validators;
using ProductParsing.Web.Auth;
using ProductParsing.Web.Extensions;
using ProductParsing.Web.Models.Catalog;

namespace ProductParsing.Web.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.CanImport)]
    public class ImportController(IImportService service) : Controller
    {
        private readonly IImportService _service = service;

        [HttpPost("/import")]
        public async Task<IActionResult> Import(ImportFormModel form)
        {
            if (ModelState.IsValid == false)
            {
                TempData.SetImportError(ModelState.FirstError() ?? ImportProductsValidator.InvalidUrlMessage, form.ListingUrl);
                return RedirectToAction(nameof(CatalogController.Index), "Catalog");
            }

            var response = await _service.ImportProducts(new ImportProductsRequest { ListingUrl = form.ListingUrl });

            if (response.Success)
            {
                TempData.SetImportReport(ImportReportViewModel.From(response.Result!));
            }
            else
            {
                TempData.SetImportError(response.Message!, form.ListingUrl);
            }

            return RedirectToAction(nameof(CatalogController.Index), "Catalog");
        }
    }
}
