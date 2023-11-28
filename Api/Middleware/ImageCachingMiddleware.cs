using EasyCaching.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;
using ProtoBuf.Serializers;
using System.Threading.Tasks;

namespace Api.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ImageCachingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IEasyCachingProviderFactory _cacheFactory;

        public ImageCachingMiddleware(RequestDelegate next, IEasyCachingProviderFactory cacheFactory)
        {
            _next = next;
            _cacheFactory = cacheFactory;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var endpointFeature = httpContext.Features.Get<IEndpointFeature>()?.Endpoint;
            var isImagePreview = endpointFeature?.DisplayName?.Contains("GetImagePreview");

            if (isImagePreview == true)
            {
                var cacheKey = httpContext.Request.QueryString.Value;
                var _provider = _cacheFactory.GetCachingProvider("disk");
                var cached = _provider.Get<CachedResponse>(cacheKey);
                if (cached.HasValue)
                {
                    //httpContext.Response.Body = new MemoryStream(cached.Value.Body);
                    await httpContext.Response.BodyWriter.WriteAsync(cached.Value.Body);
                    httpContext.Response.Headers.ContentLength = cached.Value.ContentLength;
                    httpContext.Response.Headers.ContentType = cached.Value.ContentType;
                    return;
                }
                else
                {
                    Stream originalBody = httpContext.Response.Body;
                    var shimStream = new MemoryStream();
                    httpContext.Response.Body = shimStream;

                    await _next(httpContext);

                    httpContext.Response.Body.Position = 0;

                    using var reader = new BinaryReader(httpContext.Response.Body);
                    var bytes = reader.ReadBytes((int)httpContext.Response.Body.Length);

                    var contentLength = httpContext.Response.Headers.ContentLength;
                    var contentType = httpContext.Response.Headers.ContentType.FirstOrDefault();

                    var data = new CachedResponse
                    {
                        Body = bytes,
                        ContentLength = contentLength,
                        ContentType = contentType,
                    };

                    _provider.Set<CachedResponse>(cacheKey, data, TimeSpan.FromHours(1));

                    httpContext.Response.Body.Position = 0;
                }
            }
            else
            {
                await _next(httpContext);
            }
        }
    }

    public class CachedResponse
    {
        public long? ContentLength
        {
            get; set;
        }

        public string ContentType
        {
            get; set;
        }

        public byte[] Body
        {
            get; set;
        }
    }
}