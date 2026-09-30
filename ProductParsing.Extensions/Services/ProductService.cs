using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProductParsing.Extensions.Common;
using ProductParsing.Extensions.Data;
using ProductParsing.Extensions.Extensions;
using ProductParsing.Extensions.Helpers;
using ProductParsing.Extensions.Models.Products;
using ProductParsing.Extensions.Repositories;
using ProductParsing.Extensions.Requests.Products;

namespace ProductParsing.Extensions.Services
{
    public interface IProductService
    {
        Task<PagedResponse<ProductCardModel>> GetCatalog(int page);
        Task<PagedResponse<AdminProductModel>> GetAdminProducts(int page);
        Task<ServiceResponse<ProductEditModel>> GetProductForEdit(int ID);
        Task<ServiceResponse<ProductImageModel>> GetProductImage(int ID);
        Task<ServiceResponse<int>> CreateProduct(CreateProductRequest request);
        Task<ServiceResponse> UpdateProduct(UpdateProductRequest request);
        Task<ServiceResponse> DeleteProduct(int ID);
    }

    public class ProductService(ProductParsingContext db,
                                IProductRepository repo,
                                IValidator<CreateProductRequest> createValidator,
                                IValidator<UpdateProductRequest> updateValidator) : IProductService
    {
        public const int PageSize = 20;
        public const string NotFoundMessage = "Товар не знайдено.";
        public const string ImageNotFoundMessage = "Зображення не знайдено.";

        private readonly ProductParsingContext _db = db;
        private readonly IProductRepository _repo = repo;
        private readonly IValidator<CreateProductRequest> _createValidator = createValidator;
        private readonly IValidator<UpdateProductRequest> _updateValidator = updateValidator;

        public async Task<PagedResponse<ProductCardModel>> GetCatalog(int page)
        {
            return await _repo.GetCatalogPage(page, PageSize);
        }

        public async Task<PagedResponse<AdminProductModel>> GetAdminProducts(int page)
        {
            return await _repo.GetAdminPage(page, PageSize);
        }

        public async Task<ServiceResponse<ProductEditModel>> GetProductForEdit(int ID)
        {
            var product = await _repo.GetForEdit(ID);

            if (product is null)
            {
                return NotFound<ProductEditModel>(NotFoundMessage);
            }

            return new() { Success = true, Result = product };
        }

        public async Task<ServiceResponse<ProductImageModel>> GetProductImage(int ID)
        {
            var image = await _repo.GetImage(ID);

            if (image is null)
            {
                return NotFound<ProductImageModel>(ImageNotFoundMessage);
            }

            return new() { Success = true, Result = image };
        }

        public async Task<ServiceResponse<int>> CreateProduct(CreateProductRequest request)
        {
            var validation = await _createValidator.ValidateAsync(request);

            if (!validation.IsValid)
            {
                return validation.ToResponse<int>();
            }

            var now = DateTime.UtcNow;
            var product = new Product
            {
                Name = request.Name.Trim(),
                Description = request.Description.NormalizeDescription(),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            if (request.Image is { Length: > 0 } image)
            {
                product.SetImage(image, ImageContentInspector.DetectContentType(image)!);
            }

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return new() { Success = true, Result = product.Id };
        }

        public async Task<ServiceResponse> UpdateProduct(UpdateProductRequest request)
        {
            var validation = await _updateValidator.ValidateAsync(request);

            if (!validation.IsValid)
            {
                return validation.ToResponse();
            }

            var product = await _repo.Get(request.ID, includeImage: request.ImageAction != ImageAction.Keep);

            if (product is null)
            {
                return NotFound<object>(NotFoundMessage);
            }

            product.Name = request.Name.Trim();
            product.Description = request.Description.NormalizeDescription();
            product.UpdatedAtUtc = DateTime.UtcNow;

            if (request.ImageAction == ImageAction.Replace)
            {
                product.SetImage(request.Image!, ImageContentInspector.DetectContentType(request.Image!)!);
            }
            else if (request.ImageAction == ImageAction.Remove)
            {
                product.Image = null;
            }

            await _db.SaveChangesAsync();

            return new() { Success = true };
        }

        public async Task<ServiceResponse> DeleteProduct(int ID)
        {
            var product = await _repo.Get(ID);

            if (product is null)
            {
                return NotFound<object>(NotFoundMessage);
            }

            _db.Products.Remove(product);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound<object>(NotFoundMessage);
            }

            return new() { Success = true };
        }

        private static ServiceResponse<T> NotFound<T>(string message)
        {
            return new() { Success = false, Message = message, ResponseCode = StatusCodes.Status404NotFound };
        }
    }
}
