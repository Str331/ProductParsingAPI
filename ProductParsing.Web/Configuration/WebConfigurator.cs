using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProductParsing.Extensions.Common;
using ProductParsing.Extensions.Data;
using ProductParsing.Extensions.Options;
using ProductParsing.Web.Auth;
using ProductParsing.Web.Middleware;

namespace ProductParsing.Web.Configuration
{
    public static class WebConfigurator
    {
        public const string _authCookieName = "ProductParsing.Auth";

        public static WebApplicationBuilder ConfigureWeb(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

            #region Options

            builder.Services.AddOptions<DatabaseOptions>().BindConfiguration("Database");
            builder.Services.AddOptions<SeedOptions>().BindConfiguration("Seed");
            builder.Services.AddOptions<ImportOptions>().BindConfiguration("Import").ValidateDataAnnotations().ValidateOnStart();

            #endregion

            #region Authentication

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = _authCookieName;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                    options.LoginPath = "/account/login";
                    options.LogoutPath = "/account/logout";
                    options.AccessDeniedPath = "/account/access-denied";
                });

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy(AuthorizationPolicies.CanImport, policy => policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.User)))
                .AddPolicy(AuthorizationPolicies.CanManageProducts, policy => policy.RequireRole(nameof(UserRole.Admin)));

            #endregion

            return builder;
        }

        public static WebApplication ConfigureApplicationWeb(this WebApplication app)
        {
            app.UseMiddleware<SecurityHeadersMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllers().WithStaticAssets();

            return app;
        }

        public static async Task InitializeDatabase(this WebApplication app)
        {
            await using var scope = app.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
        }
    }
}
