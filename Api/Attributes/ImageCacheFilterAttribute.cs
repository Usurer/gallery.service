using Api.Middleware;
using EasyCaching.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ImageCacheFilterAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        public bool IsReusable => true;

        public int Order
        {
            get; set;
        }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var cacheFactory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            var provider = cacheFactory.GetCachingProvider("disk");

            return new ImageCacheFilter(provider);
        }
    }

    public class ImageCacheFilter : IActionFilter
    {
        private IEasyCachingProvider _provider;

        public ImageCacheFilter(IEasyCachingProvider provider)
        {
            _provider = provider;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var cacheKey = context.HttpContext.Request.QueryString.Value;

            var fsResult = context.Result as FileStreamResult;
            var resultStream = fsResult.FileStream;

            using var reader = new BinaryReader(resultStream);
            var bytes = reader.ReadBytes((int)resultStream.Length);

            var contentLength = context.HttpContext.Response.Headers.ContentLength;
            var contentType = context.HttpContext.Response.Headers.ContentType.FirstOrDefault();

            var data = new CachedResponse
            {
                Body = bytes,
                ContentLength = contentLength,
                ContentType = contentType,
            };

            var cacheDuration = 60;
            _provider.Set<CachedResponse>(cacheKey, data, TimeSpan.FromMinutes(cacheDuration));
            resultStream.Position = 0;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var cacheKey = context.HttpContext.Request.QueryString.Value;
            var cached = _provider.Get<CachedResponse>(cacheKey);

            if (cached.HasValue)
            {
                // TODO: Set Http Cache header
                context.HttpContext.Response.Headers.ContentLength = cached.Value.ContentLength;
                context.HttpContext.Response.Headers.ContentType = cached.Value.ContentType;

                // TODO: Figure out how to make client show images with 304 response code
                //httpContext.Response.StatusCode = StatusCodes.Status304NotModified;

                context.Result = new FileContentResult(cached.Value.Body, cached.Value.ContentType == null ? "image/jpg" : cached.Value.ContentType);
            }
        }
    }
}