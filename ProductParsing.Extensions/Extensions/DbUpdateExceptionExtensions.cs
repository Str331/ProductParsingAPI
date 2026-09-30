using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ProductParsing.Extensions.Extensions
{
    public static class DbUpdateExceptionExtensions
    {
        private const int UniqueIndexViolation = 2601;
        private const int UniqueConstraintViolation = 2627;

        public static bool IsUniqueViolation(this DbUpdateException exception, string indexName)
        {
            return exception.InnerException is SqlException { Number: UniqueIndexViolation or UniqueConstraintViolation } sql
                && sql.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
