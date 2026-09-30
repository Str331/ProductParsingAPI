namespace ProductParsing.Web.Middleware
{
    public class SecurityHeadersMiddleware(RequestDelegate next)
    {
        private const string ContentSecurityPolicy =
            "default-src 'self'; img-src 'self' data:; style-src 'self'; script-src 'self'; object-src 'none'; " +
            "frame-ancestors 'none'; base-uri 'self'; form-action 'self'";

        private readonly RequestDelegate _next = next;

        public Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(SetHeaders, context);

            return _next(context);
        }

        private static Task SetHeaders(object state)
        {
            var headers = ((HttpContext)state).Response.Headers;

            headers.XFrameOptions = "DENY";
            headers.XContentTypeOptions = "nosniff";
            headers.ContentSecurityPolicy = ContentSecurityPolicy;
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            return Task.CompletedTask;
        }
    }
}
