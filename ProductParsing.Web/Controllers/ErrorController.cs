using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductParsing.Web.Models;

namespace ProductParsing.Web.Controllers
{
    [AllowAnonymous, IgnoreAntiforgeryToken, Route("error")]
    public class ErrorController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier, StatusCode = StatusCodes.Status500InternalServerError });
        }

        [Route("{statusCode:int}")]
        public IActionResult Status(int statusCode)
        {
            Response.StatusCode = statusCode is >= 400 and <= 599 ? statusCode : StatusCodes.Status404NotFound;

            if (Response.StatusCode == StatusCodes.Status404NotFound)
            {
                return View("NotFound");
            }

            return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier, StatusCode = Response.StatusCode });
        }
    }
}
