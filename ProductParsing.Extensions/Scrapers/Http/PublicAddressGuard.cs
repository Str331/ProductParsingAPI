using System.Net;
using System.Net.Sockets;

namespace ProductParsing.Extensions.Scrapers.Http
{
    public static class PublicAddressGuard
    {
        public static bool IsPublic(IPAddress address)
        {
            if (address.IsIPv4MappedToIPv6)
            {
                address = address.MapToIPv4();
            }

            return address.AddressFamily switch
            {
                AddressFamily.InterNetwork => IsPublicIPv4(address.GetAddressBytes()),
                AddressFamily.InterNetworkV6 => IsPublicIPv6(address),
                _ => false
            };
        }

        private static bool IsPublicIPv4(byte[] b)
        {
            return !(b[0] == 0
                     || b[0] == 10
                     || (b[0] == 100 && (b[1] & 0xC0) == 64)
                     || b[0] == 127
                     || (b[0] == 169 && b[1] == 254)
                     || (b[0] == 172 && (b[1] & 0xF0) == 16)
                     || (b[0] == 192 && b[1] == 168)
                     || b[0] >= 224);
        }

        private static bool IsPublicIPv6(IPAddress address)
        {
            if (address.Equals(IPAddress.IPv6Any) || address.Equals(IPAddress.IPv6Loopback))
            {
                return false;
            }

            var b = address.GetAddressBytes();
            var uniqueLocal = (b[0] & 0xFE) == 0xFC;
            var linkLocal = b[0] == 0xFE && (b[1] & 0xC0) == 0x80;
            var multicast = b[0] == 0xFF;

            return !(uniqueLocal || linkLocal || multicast);
        }
    }
}
