using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductParsing.Extensions.Requests.Auth.Login;
using ProductParsing.Extensions.Services;
using ProductParsing.Web.Auth;
using ProductParsing.Web.Extensions;
using ProductParsing.Web.Models.Account;

namespace ProductParsing.Web.Controllers
{
    [Route("account")]
    public class AccountController(IAuthService service) : Controller
    {
        private readonly IAuthService _service = service;

        [HttpGet("login"), AllowAnonymous]
        public ActionResult Login(string? returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost("login"), AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                return View(model);
            }

            var response = await _service.Login(new LoginRequest { UserName = model.UserName, Password = model.Password });

            if (response.Success == false)
            {
                ModelState.AddError(response);

                return View(model);
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, UserPrincipal.Create(response.Result!));

            return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/");
        }

        [HttpPost("logout"), Authorize]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return LocalRedirect("/");
        }

        [HttpGet("access-denied"), AllowAnonymous]
        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}
