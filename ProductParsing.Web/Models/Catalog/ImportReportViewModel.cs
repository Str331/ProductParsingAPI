using ProductParsing.Extensions.Requests.Import;
using System.Text.Json.Serialization;

namespace ProductParsing.Web.Models.Catalog
{
    public class ImportReportViewModel
    {
        public const int MaxListedIssues = 10;
        private const int MaxReasonLength = 200;
        private const int MaxUrlLength = 200;

        public int Found { get; init; }
        public int Added { get; init; }
        public int Skipped { get; init; }
        public int FailedCount { get; init; }
        public int WarningCount { get; init; }
        public bool TimedOut { get; init; }
        public List<ImportIssueViewModel> Failed { get; init; } = [];
        public List<ImportIssueViewModel> Warnings { get; init; } = [];

        [JsonIgnore]
        public int HiddenFailures => FailedCount - Failed.Count;

        [JsonIgnore]
        public int HiddenWarnings => WarningCount - Warnings.Count;

        [JsonIgnore]
        public bool HasProblems => FailedCount > 0 || TimedOut;

        public static ImportReportViewModel From(ImportProductsResponse response)
        {
            return new ImportReportViewModel
            {
                Found = response.Found,
                Added = response.Added,
                Skipped = response.Skipped,
                FailedCount = response.Failed.Count,
                WarningCount = response.Warnings.Count,
                TimedOut = response.TimedOut,
                Failed = Top(response.Failed),
                Warnings = Top(response.Warnings)
            };
        }

        private static List<ImportIssueViewModel> Top(List<ImportIssue> issues)
        {
            return issues.Take(MaxListedIssues)
                         .Select(i => new ImportIssueViewModel { Url = Cut(i.Url, MaxUrlLength), Reason = Cut(i.Reason, MaxReasonLength) })
                         .ToList();
        }

        private static string Cut(string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";
        }
    }
}
