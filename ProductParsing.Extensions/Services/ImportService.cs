using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductParsing.Extensions.Common;
using ProductParsing.Extensions.Data;
using ProductParsing.Extensions.Extensions;
using ProductParsing.Extensions.Options;
using ProductParsing.Extensions.Repositories;
using ProductParsing.Extensions.Requests.Import;
using ProductParsing.Extensions.Scrapers;
using ProductParsing.Extensions.Scrapers.Http;
using ProductParsing.Extensions.Scrapers.Robots;

namespace ProductParsing.Extensions.Services
{
    public interface IImportService
    {
        Task<ServiceResponse<ImportProductsResponse>> ImportProducts(ImportProductsRequest request);
    }

    public class ImportService(ProductParsingContext db,
                               IStoreScraper scraper,
                               IRobotsTxtService robots,
                               IImageDownloader imageDownloader,
                               IProductRepository repo,
                               IValidator<ImportProductsRequest> validator,
                               IOptions<ImportOptions> options,
                               ILogger<ImportService> logger) : IImportService
    {
        public const string NotPublicMessage = "Потрібна публічна адреса сайту: http або https, без порту та IP-адреси.";

        private const string StoreTimeout = "Магазин не відповів вчасно.";
        private const string RobotsDisallowed = "Сторінка закрита для ботів у robots.txt.";

        private readonly ProductParsingContext _db = db;
        private readonly IStoreScraper _scraper = scraper;
        private readonly IRobotsTxtService _robots = robots;
        private readonly IImageDownloader _imageDownloader = imageDownloader;
        private readonly IProductRepository _repo = repo;
        private readonly IValidator<ImportProductsRequest> _validator = validator;
        private readonly ImportOptions _options = options.Value;
        private readonly ILogger<ImportService> _logger = logger;

        public async Task<ServiceResponse<ImportProductsResponse>> ImportProducts(ImportProductsRequest request)
        {
            var validation = await _validator.ValidateAsync(request);

            if (!validation.IsValid)
            {
                return validation.ToResponse<ImportProductsResponse>();
            }

            var listingUrl = new Uri(request.ListingUrl.Trim());

            if (!PublicUrlPolicy.IsAllowed(listingUrl))
            {
                return Error(NotPublicMessage);
            }

            using var timeout = new CancellationTokenSource(_options.Timeout);

            IReadOnlyList<Uri> links;

            try
            {
                if (!await _robots.IsAllowedAsync(listingUrl, timeout.Token))
                {
                    return Error($"Не вдалося обробити сторінку каталогу: {RobotsDisallowed}");
                }

                links = await _scraper.GetProductLinksAsync(listingUrl, timeout.Token);
            }
            catch (ScrapingException ex)
            {
                return Error($"Не вдалося обробити сторінку каталогу: {ex.Reason}");
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
                return Error("Не вдалося обробити сторінку каталогу: магазин не відповів вчасно.");
            }

            var candidates = links.DistinctBy(url => url.AbsoluteUri, StringComparer.OrdinalIgnoreCase)
                                  .Take(_options.MaxProductsPerImport)
                                  .ToList();
            var existing = await _repo.GetExistingSourceUrls(candidates.Select(url => url.AbsoluteUri).ToList());
            var toScrape = candidates.Where(url => !existing.Contains(url.AbsoluteUri)).ToList();

            var collector = new ImportCollector();
            var timedOut = await ScrapeAll(toScrape, collector, timeout);
            var (added, skippedByRace) = await Save(collector.Products);

            var response = collector.ToResponse(candidates.Count, added, existing.Count + skippedByRace, timedOut);

            _logger.LogInformation("Import from {ListingUrl}: found {Found}, added {Added}, skipped {Skipped}, failed {Failed}, warnings {Warnings}, timed out {TimedOut}",
                                   listingUrl, response.Found, response.Added, response.Skipped, response.Failed.Count, response.Warnings.Count, response.TimedOut);

            return new() { Success = true, Result = response };
        }

        private async Task<bool> ScrapeAll(List<Uri> urls, ImportCollector collector, CancellationTokenSource timeout)
        {
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism, CancellationToken = timeout.Token };

            try
            {
                await Parallel.ForEachAsync(urls, parallelOptions, (url, token) => ImportOne(url, collector, token));
                return false;
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
                return true;
            }
        }

        private async ValueTask ImportOne(Uri url, ImportCollector collector, CancellationToken ct)
        {
            ScrapedProduct scraped;

            try
            {
                if (!await _robots.IsAllowedAsync(url, ct))
                {
                    collector.AddFailure(url, RobotsDisallowed);
                    return;
                }

                scraped = await _scraper.ScrapeProductAsync(url, ct);
            }
            catch (ScrapingException ex)
            {
                _logger.LogWarning("Product {ProductUrl} failed: {Reason}", url, ex.Reason);
                collector.AddFailure(url, ex.Reason);
                return;
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                collector.AddFailure(url, StoreTimeout);
                return;
            }

            var invalid = Check(scraped, url);

            if (invalid is not null)
            {
                collector.AddFailure(url, invalid);
                return;
            }

            var now = DateTime.UtcNow;
            var product = new Product
            {
                Name = scraped.Name.Trim(),
                Description = scraped.Description.NormalizeDescription(),
                SourceUrl = url.AbsoluteUri,
                SourceHost = url.Host,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            await AttachImage(product, url, scraped.ImageUrl, collector, ct);
            collector.AddProduct(product);
        }

        private async Task AttachImage(Product product, Uri url, Uri? imageUrl, ImportCollector collector, CancellationToken ct)
        {
            if (imageUrl is null)
            {
                collector.AddWarning(url, "Зображення не знайдено на сторінці товару.");
                return;
            }

            if (imageUrl.AbsoluteUri.Length > ProductImage.SourceUrlMaxLength)
            {
                collector.AddWarning(url, "Зображення відхилено: адреса зображення занадто довга.");
                return;
            }

            try
            {
                var image = await _imageDownloader.DownloadAsync(imageUrl, ct);
                product.SetImage(image.Content, image.ContentType, image.SourceUrl.AbsoluteUri);
            }
            catch (ScrapingException ex)
            {
                collector.AddWarning(url, $"Зображення не завантажено: {ex.Reason}");
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                collector.AddWarning(url, "Зображення не завантажено: магазин не відповів вчасно.");
            }
        }

        private async Task<(int Added, int SkippedByRace)> Save(List<Product> products)
        {
            if (products.Count == 0)
            {
                return (0, 0);
            }

            _db.Products.AddRange(products);

            try
            {
                await _db.SaveChangesAsync();
                return (products.Count, 0);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation(ProductParsingContext._sourceUrlIndexName))
            {
                _db.ChangeTracker.Clear();

                var existing = await _repo.GetExistingSourceUrls(products.Select(p => p.SourceUrl!).ToList());
                var remaining = products.Where(p => !existing.Contains(p.SourceUrl!)).ToList();

                _db.Products.AddRange(remaining);
                await _db.SaveChangesAsync();

                return (remaining.Count, products.Count - remaining.Count);
            }
        }

        private static string? Check(ScrapedProduct scraped, Uri url)
        {
            if (string.IsNullOrWhiteSpace(scraped.Name))
            {
                return "Назва обов'язкова.";
            }

            if (scraped.Name.Trim().Length > Product.NameMaxLength)
            {
                return $"Назва не може перевищувати {Product.NameMaxLength} символів.";
            }

            if (scraped.Description?.Trim().Length > Product.DescriptionMaxLength)
            {
                return $"Опис не може перевищувати {Product.DescriptionMaxLength} символів.";
            }

            if (url.AbsoluteUri.Length > Product.SourceUrlMaxLength)
            {
                return $"Адреса джерела не може перевищувати {Product.SourceUrlMaxLength} символів.";
            }

            return null;
        }

        private static ServiceResponse<ImportProductsResponse> Error(string message)
        {
            return new() { Success = false, Message = message, Field = nameof(ImportProductsRequest.ListingUrl), ResponseCode = StatusCodes.Status400BadRequest };
        }
    }
}
