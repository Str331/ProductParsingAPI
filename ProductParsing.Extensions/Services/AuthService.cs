using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ProductParsing.Extensions.Common;
using ProductParsing.Extensions.Data;
using ProductParsing.Extensions.Repositories;
using ProductParsing.Extensions.Requests.Auth.Login;

namespace ProductParsing.Extensions.Services
{
    public interface IAuthService
    {
        Task<ServiceResponse<LoginResponse>> Login(LoginRequest request);
    }

    public class AuthService(IUserRepository repo, IPasswordHasher<User> hasher) : IAuthService
    {
        public const string InvalidCredentialsMessage = "Невірний логін або пароль.";

        private readonly IUserRepository _userService = repo;
        private readonly IPasswordHasher<User> _hasher = hasher;

        public async Task<ServiceResponse<LoginResponse>> Login(LoginRequest request)
        {
            var userName = request.UserName?.Trim() ?? string.Empty;
            var password = request.Password ?? string.Empty;
            var user = userName.Length == 0 ? null : await _userService.Find(userName);

            if (user is null || Verify(user, password) == false)
            {
                return InvalidCredentials();
            }

            return new()
            {
                Success = true,
                Result = new LoginResponse
                {
                    ID = user.Id,
                    UserName = user.UserName,
                    Role = user.Role.Name
                }
            };
        }

        private bool Verify(User user, string password)
        {
            try
            {
                return _hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static ServiceResponse<LoginResponse> InvalidCredentials()
        {
            return new() { Success = false, Message = InvalidCredentialsMessage, ResponseCode = StatusCodes.Status401Unauthorized };
        }
    }
}
