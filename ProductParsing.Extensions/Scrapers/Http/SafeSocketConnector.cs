using System.Net;
using System.Net.Sockets;

namespace ProductParsing.Extensions.Scrapers.Http
{
    public static class SafeSocketConnector
    {
        public static async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, CancellationToken ct)
        {
            var endpoint = context.DnsEndPoint;
            IPAddress[] addresses = IPAddress.TryParse(endpoint.Host, out var literal) ? [literal] : await Dns.GetHostAddressesAsync(endpoint.Host, ct);
            var allowed = addresses.Where(PublicAddressGuard.IsPublic).ToArray();

            if (allowed.Length == 0)
            {
                throw new ScrapingException($"Заборонена мережева адреса для {endpoint.Host}.");
            }

            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };

            try
            {
                await socket.ConnectAsync(allowed, endpoint.Port, ct);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
    }
}
