using EasyCaching.Core;
using Microsoft.AspNetCore.OutputCaching;

namespace Api.Middleware
{
    public class ImageCachePolicy : IOutputCachePolicy
    {
        private readonly IEasyCachingProvider _cacheProvider;

        public ImageCachePolicy(IEasyCachingProviderFactory cacheFactory)
        {
            _cacheProvider = cacheFactory.GetCachingProvider("disk");
        }

        public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }
    }
}