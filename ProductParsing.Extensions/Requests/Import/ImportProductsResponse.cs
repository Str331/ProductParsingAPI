namespace ProductParsing.Extensions.Requests.Import
{
    public class ImportProductsResponse
    {
        public int Found { get; init; }
        public int Added { get; init; }
        public int Skipped { get; init; }
        public bool TimedOut { get; init; }
        public List<ImportIssue> Failed { get; init; } = [];
        public List<ImportIssue> Warnings { get; init; } = [];
    }
}
