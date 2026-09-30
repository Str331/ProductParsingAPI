using Microsoft.EntityFrameworkCore;
using ProductParsing.Extensions.Common;
using ProductParsing.Extensions.Data;
using ProductParsing.Extensions.Models.Products;

namespace ProductParsing.Extensions.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> Get(int id, bool includeImage = false);
        Task<ProductEditModel?> GetForEdit(int id);
        Task<ProductImageModel?> GetImage(int productId);
        Task<PagedResponse<ProductCardModel>> GetCatalogPage(int page, int pageSize);
        Task<PagedResponse<AdminProductModel>> GetAdminPage(int page, int pageSize);
        Task<HashSet<string>> GetExistingSourceUrls(ICollection<string> sourceUrls);
    }

    public class ProductRepository(ProductParsingContext db) : IProductRepository
    {
        private readonly ProductParsingContext _db = db;

        public async Task<Product?> Get(int id, bool includeImage = false)
        {
            IQueryable<Product> query = _db.Products;

            if (includeImage)
            {
                query = query.Include(p => p.Image);
            }

            return await query.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<ProductEditModel?> GetForEdit(int id)
        {
            var row = await _db.Products.AsNoTracking()
                                        .Where(p => p.Id == id)
                                        .Select(p => new
                                        {
                                            p.Id,
                                            p.Name,
                                            p.Description,
                                            p.SourceUrl,
                                            HasImage = p.Image != null
                                        })
                                        .FirstOrDefaultAsync();

            if (row is null)
            {
                return null;
            }

            return new ProductEditModel
            {
                ID = row.Id,
                Name = row.Name,
                Description = row.Description,
                SourceUrl = row.SourceUrl,
                HasImage = row.HasImage
            };
        }

        public async Task<ProductImageModel?> GetImage(int productId)
        {
            return await _db.ProductImages.AsNoTracking()
                                          .Where(i => i.ProductId == productId)
                                          .Select(i => new ProductImageModel { Content = i.Content, ContentType = i.ContentType })
                                          .FirstOrDefaultAsync();
        }

        public async Task<PagedResponse<ProductCardModel>> GetCatalogPage(int page, int pageSize)
        {
            var query = NewestFirst().Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.SourceUrl,
                p.SourceHost,
                HasImage = p.Image != null
            });

            var (safePage, total, rows) = await ToPage(query, page, pageSize);

            return new PagedResponse<ProductCardModel>
            {
                Page = safePage,
                PageSize = pageSize,
                Total = total,
                Data = rows.Select(r => new ProductCardModel
                {
                    ID = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    SourceUrl = r.SourceUrl,
                    SourceHost = r.SourceHost,
                    HasImage = r.HasImage
                }).ToList()
            };
        }

        public async Task<PagedResponse<AdminProductModel>> GetAdminPage(int page, int pageSize)
        {
            var query = NewestFirst().Select(p => new
            {
                p.Id,
                p.Name,
                p.SourceHost,
                p.CreatedAtUtc,
                HasImage = p.Image != null
            });

            var (safePage, total, rows) = await ToPage(query, page, pageSize);

            return new PagedResponse<AdminProductModel>
            {
                Page = safePage,
                PageSize = pageSize,
                Total = total,
                Data = rows.Select(r => new AdminProductModel
                {
                    ID = r.Id,
                    Name = r.Name,
                    SourceHost = r.SourceHost,
                    CreatedAtUtc = DateTime.SpecifyKind(r.CreatedAtUtc, DateTimeKind.Utc),
                    HasImage = r.HasImage
                }).ToList()
            };
        }

        public async Task<HashSet<string>> GetExistingSourceUrls(ICollection<string> sourceUrls)
        {
            if (sourceUrls.Count == 0)
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            var candidates = sourceUrls.ToList();
            var existing = await _db.Products.Where(p => p.SourceUrl != null && candidates.Contains(p.SourceUrl))
                                             .Select(p => p.SourceUrl!)
                                             .ToListAsync();

            return existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private IQueryable<Product> NewestFirst()
        {
            return _db.Products.AsNoTracking().OrderByDescending(p => p.CreatedAtUtc).ThenByDescending(p => p.Id);
        }

        private static async Task<(int Page, int Total, List<T> Rows)> ToPage<T>(IQueryable<T> query, int page, int pageSize)
        {
            var safePage = Math.Max(1, page);
            var total = await query.CountAsync();

            var offset = (long)(safePage - 1) * pageSize;

            if (offset >= total)
            {
                return (safePage, total, []);
            }

            var rows = await query.Skip((int)offset).Take(pageSize).ToListAsync();

            return (safePage, total, rows);
        }
    }
}
