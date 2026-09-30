using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductParsing.Extensions.Services;
using ProductParsing.Web.Auth;
using ProductParsing.Web.Extensions;
using ProductParsing.Web.Models.Catalog;

namespace ProductParsing.Web.Controllers
{
    [AllowAnonymous]
    public class CatalogController(IProductService service, IAuthorizationService authorization) : Controller
    {
        private readonly IProductService _service = service;
        private readonly IAuthorizationService _authorization = authorization;

        [HttpGet("/")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var model = new CatalogViewModel
            {
                Products = await _service.GetCatalog(page),
                CanImport = await IsAllowed(AuthorizationPolicies.CanImport),
                CanManageProducts = await IsAllowed(AuthorizationPolicies.CanManageProducts),
                ImportForm = new ImportFormModel { ListingUrl = TempData.GetImportListingUrl() ?? string.Empty },
                ImportReport = TempData.GetImportReport(),
                ImportError = TempData.GetImportError()
            };

            return View(model);
        }

        private async Task<bool> IsAllowed(string policy)
        {
            return (await _authorization.AuthorizeAsync(User, policy)).Succeeded;
        }
    }
}
