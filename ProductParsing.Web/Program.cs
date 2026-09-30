using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductParsing.Extensions.Data;
using ProductParsing.Extensions.Extensions;
using ProductParsing.Extensions.Repositories;
using ProductParsing.Extensions.Services;
using ProductParsing.Extensions.Validators;
using ProductParsing.Web.Configuration;

var builder = WebApplication.CreateBuilder(args).ConfigureWeb();

builder.Services.AddDbContext<ProductParsingContext>((provider, options) =>
{
    var connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString("Default")
        ?? throw new Exception("No connection string is provided for the ConnectionStrings:Default section.");

    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
});

#region Repos

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

#endregion

#region Services

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<DatabaseInitializer>();

#endregion

builder.Services.AddScrapers();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

var app = builder.Build().ConfigureApplicationWeb();

await app.InitializeDatabase();

await app.RunAsync();
