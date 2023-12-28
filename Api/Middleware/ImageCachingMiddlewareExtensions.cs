namespace Api.Middleware
{
    public static class ImageCachingMiddlewareExtensions
    {
        public static IApplicationBuilder UseImageCachingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ImageCachingMiddleware>();
        }
    }
}