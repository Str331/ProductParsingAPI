using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using ProductParsing.Extensions.Options;
using ProductParsing.Extensions.Scrapers;
using ProductParsing.Extensions.Scrapers.Http;
using ProductParsing.Extensions.Scrapers.Robots;
using System.Net;

namespace ProductParsing.Extensions.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScrapers(this IServiceCollection services)
        {
            services.AddOptions<ScrapingOptions>().BindConfiguration("Scraping").ValidateDataAnnotations().ValidateOnStart();

            services.AddHttpClient(HttpContentFetcher.HttpClientName, ConfigureClient)
                    .ConfigurePrimaryHttpMessageHandler(CreateHandler)
                    .AddStandardResilienceHandler()
                    .Configure((HttpStandardResilienceOptions resilience, IServiceProvider provider) =>
                        ConfigureResilience(resilience, provider.GetRequiredService<IOptions<ScrapingOptions>>().Value));

            services.AddTransient<HttpContentFetcher>();
            services.AddTransient<IHtmlDocumentLoader, HtmlDocumentLoader>();
            services.AddTransient<IImageDownloader, HttpImageDownloader>();

            services.AddTransient<IStoreScraper, StoreScraper>();
            services.AddScoped<IRobotsTxtService, RobotsTxtService>();

            return services;
        }

        private static void ConfigureClient(IServiceProvider provider, HttpClient client)
        {
            var options = provider.GetRequiredService<IOptions<ScrapingOptions>>().Value;

            client.Timeout = Timeout.InfiniteTimeSpan;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("uk-UA,uk;q=0.9");
        }

        private static SocketsHttpHandler CreateHandler()
        {
            return new SocketsHttpHandler
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.All,
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                ConnectCallback = SafeSocketConnector.ConnectAsync
            };
        }

        private static void ConfigureResilience(HttpStandardResilienceOptions resilience, ScrapingOptions options)
        {
            resilience.Retry.MaxRetryAttempts = options.MaxRetryAttempts;
            resilience.Retry.Delay = options.RetryBaseDelay;
            resilience.AttemptTimeout.Timeout = options.AttemptTimeout;
            resilience.TotalRequestTimeout.Timeout = options.AttemptTimeout * 3;
            resilience.CircuitBreaker.SamplingDuration = options.AttemptTimeout * 2;
        }
    }
}
