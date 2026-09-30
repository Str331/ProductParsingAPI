namespace ProductParsing.Extensions.Helpers
{
    public static class ImageContentTypes
    {
        public const string _jpeg = "image/jpeg";
        public const string _png = "image/png";
        public const string _webp = "image/webp";
        public const string _gif = "image/gif";
    }

    public static class ImageContentInspector
    {
        private static ReadOnlySpan<byte> JpegMagic => [0xFF, 0xD8, 0xFF];
        private static ReadOnlySpan<byte> PngMagic => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        private static ReadOnlySpan<byte> Gif87Magic => "GIF87a"u8;
        private static ReadOnlySpan<byte> Gif89Magic => "GIF89a"u8;
        private static ReadOnlySpan<byte> RiffMagic => "RIFF"u8;
        private static ReadOnlySpan<byte> WebpMagic => "WEBP"u8;

        public static string? DetectContentType(ReadOnlySpan<byte> content)
        {
            if (content.StartsWith(JpegMagic))
            {
                return ImageContentTypes._jpeg;
            }

            if (content.StartsWith(PngMagic))
            {
                return ImageContentTypes._png;
            }

            if (content.StartsWith(Gif87Magic) || content.StartsWith(Gif89Magic))
            {
                return ImageContentTypes._gif;
            }

            if (content.Length >= 12 && content.StartsWith(RiffMagic) && content.Slice(8, 4).SequenceEqual(WebpMagic))
            {
                return ImageContentTypes._webp;
            }

            return null;
        }
    }
}
