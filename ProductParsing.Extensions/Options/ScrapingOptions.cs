using ProductParsing.Extensions.Data;
using System.ComponentModel.DataAnnotations;

namespace ProductParsing.Extensions.Options
{
    public class ScrapingOptions
    {
        [Required]
        public string UserAgent { get; set; } = "ProductParsingBot/1.0";

        [Range(1024, 50 * 1024 * 1024)]
        public int MaxHtmlBytes { get; set; } = 10 * 1024 * 1024;

        [Range(1024, ProductImage.MaxBytes)]
        public int MaxImageBytes { get; set; } = ProductImage.MaxBytes;

        [Range(0, 10)]
        public int MaxRedirects { get; set; } = 3;

        [Range(1, 5)]
        public int MaxRetryAttempts { get; set; } = 3;

        [Range(typeof(TimeSpan), "00:00:00.001", "00:00:30")]
        public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);

        [Range(typeof(TimeSpan), "00:00:01", "00:01:00")]
        public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(10);
    }
}
