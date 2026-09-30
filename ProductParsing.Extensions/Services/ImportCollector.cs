using ProductParsing.Extensions.Data;
using ProductParsing.Extensions.Requests.Import;
using System.Collections.Concurrent;

namespace ProductParsing.Extensions.Services
{
    public class ImportCollector
    {
        private readonly ConcurrentQueue<Product> _products = new();
        private readonly ConcurrentQueue<ImportIssue> _failed = new();
        private readonly ConcurrentQueue<ImportIssue> _warnings = new();

        public List<Product> Products => _products.ToList();

        public void AddProduct(Product product)
        {
            _products.Enqueue(product);
        }

        public void AddFailure(Uri url, string reason)
        {
            _failed.Enqueue(new ImportIssue(url.AbsoluteUri, reason));
        }

        public void AddWarning(Uri url, string reason)
        {
            _warnings.Enqueue(new ImportIssue(url.AbsoluteUri, reason));
        }

        public ImportProductsResponse ToResponse(int found, int added, int skipped, bool timedOut)
        {
            return new ImportProductsResponse
            {
                Found = found,
                Added = added,
                Skipped = skipped,
                TimedOut = timedOut,
                Failed = Sorted(_failed),
                Warnings = Sorted(_warnings)
            };
        }

        private static List<ImportIssue> Sorted(IEnumerable<ImportIssue> issues)
        {
            return issues.OrderBy(i => i.Url, StringComparer.Ordinal).ToList();
        }
    }
}
