using Microsoft.AspNetCore.Authentication.Cookies;
using ProductParsing.Extensions.Requests.Auth.Login;
using System.Globalization;
using System.Security.Claims;

namespace ProductParsing.Web.Auth
{
    public static class UserPrincipal
    {
        public static ClaimsPrincipal Create(LoginResponse user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.ID.ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Role, user.Role)
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        }
    }
}
