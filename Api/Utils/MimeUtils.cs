namespace Api.Utils
{
    public static class MimeUtils
    {
        public static string ExtensionToMime(string extension)
        {
            // TODO: Verify input, right now we expect it to be a valid extension, think of https://docs.fluentvalidation.net/en/latest/
            // TODO: Fix #12 and remove Trim()
            return extension.TrimStart('.').ToLowerInvariant() switch
            {
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                _ => "image"
            };
        }
    }
}
