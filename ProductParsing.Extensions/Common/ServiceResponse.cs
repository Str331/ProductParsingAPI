namespace ProductParsing.Extensions.Common
{
    public class ServiceResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Field { get; set; }
        public int? ResponseCode { get; set; }
    }

    public class ServiceResponse<T> : ServiceResponse
    {
        public T? Result { get; set; }
    }
}
