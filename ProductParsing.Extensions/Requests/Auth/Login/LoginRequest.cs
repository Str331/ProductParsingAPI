namespace ProductParsing.Extensions.Requests.Auth.Login
{
    public class LoginRequest
    {
        public required string UserName { get; init; }
        public required string Password { get; init; }
    }
}
