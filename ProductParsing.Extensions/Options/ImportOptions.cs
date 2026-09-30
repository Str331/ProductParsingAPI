using System.ComponentModel.DataAnnotations;

namespace ProductParsing.Extensions.Options
{
    public class ImportOptions
    {
        [Range(1, 200)]
        public int MaxProductsPerImport { get; set; } = 50;

        [Range(1, 16)]
        public int MaxDegreeOfParallelism { get; set; } = 4;

        [Range(typeof(TimeSpan), "00:00:05", "00:10:00")]
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);
    }
}
