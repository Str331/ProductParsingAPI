using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductParsing.Extensions.Options;

namespace ProductParsing.Extensions.Data
{
    public class DatabaseInitializer(ProductParsingContext db,
                                     IPasswordHasher<User> hasher,
                                     IOptions<DatabaseOptions> databaseOptions,
                                     IOptions<SeedOptions> seedOptions,
                                     ILogger<DatabaseInitializer> logger)
    {
        private readonly ProductParsingContext _db = db;
        private readonly IPasswordHasher<User> _hasher = hasher;
        private readonly DatabaseOptions _databaseOptions = databaseOptions.Value;
        private readonly SeedOptions _seedOptions = seedOptions.Value;
        private readonly ILogger<DatabaseInitializer> _logger = logger;

        public async Task Initialize()
        {
            if (_databaseOptions.ApplyMigrationsOnStartup == false)
            {
                _logger.LogInformation("Automatic migrations are disabled, skipping database initialization");
                return;
            }

            await _db.Database.MigrateAsync();
            await SeedUsers();
        }

        private async Task SeedUsers()
        {
            if (_seedOptions.Users.Count == 0)
            {
                return;
            }

            var roles = await _db.Roles.AsNoTracking().ToDictionaryAsync(r => r.Name, r => r.Id, StringComparer.OrdinalIgnoreCase);
            var existing = (await _db.Users.Select(u => u.UserName).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var seed in _seedOptions.Users)
            {
                var userName = seed.UserName.Trim();

                if (existing.Add(userName))
                {
                    _db.Users.Add(CreateUser(userName, seed, roles));
                }
            }

            var added = await _db.SaveChangesAsync();
            _logger.LogInformation("Seeded {SeededUserCount} user(s)", added);
        }

        private User CreateUser(string userName, SeedUserOptions seed, Dictionary<string, int> roles)
        {
            if (userName.Length == 0 || userName.Length > User.UserNameMaxLength)
            {
                throw new InvalidOperationException($"Seed user name '{seed.UserName}' must be 1-{User.UserNameMaxLength} characters long.");
            }

            if (string.IsNullOrWhiteSpace(seed.Password))
            {
                throw new InvalidOperationException($"Seed user '{userName}' has no password configured.");
            }

            if (!roles.TryGetValue(seed.Role, out var roleId))
            {
                throw new InvalidOperationException($"Seed user '{userName}' references unknown role '{seed.Role}'.");
            }

            var user = new User { UserName = userName, RoleId = roleId };
            user.PasswordHash = _hasher.HashPassword(user, seed.Password);

            return user;
        }
    }
}
