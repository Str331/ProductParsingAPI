using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ProductParsing.Web.Models.Catalog;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace ProductParsing.Web.Extensions
{
    public static class TempDataExtensions
    {
        private const string _reportKey = "Import.Report";
        private const string _errorKey = "Import.Error";
        private const string _listingUrlKey = "Import.ListingUrl";

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        public static void SetImportReport(this ITempDataDictionary tempData, ImportReportViewModel report)
        {
            tempData[_reportKey] = JsonSerializer.Serialize(report, SerializerOptions);
        }

        public static void SetImportError(this ITempDataDictionary tempData, string message, string? listingUrl)
        {
            tempData[_errorKey] = message;
            tempData[_listingUrlKey] = listingUrl;
        }

        public static ImportReportViewModel? GetImportReport(this ITempDataDictionary tempData)
        {
            return tempData[_reportKey] is string json ? JsonSerializer.Deserialize<ImportReportViewModel>(json, SerializerOptions) : null;
        }

        public static string? GetImportError(this ITempDataDictionary tempData)
        {
            return tempData[_errorKey] as string;
        }

        public static string? GetImportListingUrl(this ITempDataDictionary tempData)
        {
            return tempData[_listingUrlKey] as string;
        }
    }
}
