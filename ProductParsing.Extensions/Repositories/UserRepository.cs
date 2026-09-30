using Microsoft.EntityFrameworkCore;
using ProductParsing.Extensions.Data;

namespace ProductParsing.Extensions.Repositories
{
    public interface IUserRepository
    {
        Task<User?> Find(string userName);
    }

    public class UserRepository(ProductParsingContext db) : IUserRepository
    {
        private readonly ProductParsingContext _db = db;

        public async Task<User?> Find(string userName)
        {
            return await _db.Users.AsNoTracking()
                                  .Include(u => u.Role)
                                  .FirstOrDefaultAsync(u => u.UserName == userName);
        }
    }
}
