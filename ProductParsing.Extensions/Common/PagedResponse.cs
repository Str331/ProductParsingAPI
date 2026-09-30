namespace ProductParsing.Extensions.Common
{
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = [];
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }

        public int TotalPages => Total == 0 ? 1 : (int)Math.Ceiling(Total / (double)PageSize);
    }
}
