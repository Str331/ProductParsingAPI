namespace ProductParsing.Extensions.Scrapers
{
    public class ScrapingException(string reason, Exception? innerException = null, int? statusCode = null) : Exception(reason, innerException)
    {
        public string Reason => Message;

        public int? StatusCode { get; } = statusCode;
    }
}
