namespace ProductParsing.Extensions.Requests.Auth.Login
{
    public class LoginResponse
    {
        public required int ID { get; init; }
        public required string UserName { get; init; }
        public required string Role { get; init; }
    }
}
