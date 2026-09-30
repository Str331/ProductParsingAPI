using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProductParsing.Extensions.Data
{
    public class ProductParsingContextFactory : IDesignTimeDbContextFactory<ProductParsingContext>
    {
        public ProductParsingContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<ProductParsingContext>()
                .UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=ProductParsing;Trusted_Connection=True;TrustServerCertificate=True")
                .Options;

            return new ProductParsingContext(options);
        }
    }
}
