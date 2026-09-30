namespace ProductParsing.Web.Extensions
{
    public static class FormFileExtensions
    {
        public static async Task<byte[]?> ToBytes(this IFormFile? file)
        {
            if (file is null || file.Length == 0)
            {
                return null;
            }

            using var buffer = new MemoryStream((int)file.Length);
            await file.CopyToAsync(buffer);

            return buffer.ToArray();
        }
    }
}
