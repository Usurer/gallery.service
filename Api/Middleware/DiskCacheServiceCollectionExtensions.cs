using EasyCaching.Core;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Api.Middleware
{
    public static class DiskCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddDiskOutputCache(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            //services.AddTransient<IConfigureOptions<OutputCacheOptions>>();

            //services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            services.TryAddSingleton<IOutputCacheStore>(sp =>
            {
                var outputCacheOptions = sp.GetRequiredService<IOptions<OutputCacheOptions>>();
                var factory = sp.GetService<IEasyCachingProviderFactory>();
                var provider = factory.GetCachingProvider("disk");

                return new DiskOutputCacheStore(provider, outputCacheOptions.Value);
            });
            return services;
        }

        public static IServiceCollection AddDiskOutputCache(this IServiceCollection services, Action<OutputCacheOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configureOptions);

            services.Configure(configureOptions);
            services.AddDiskOutputCache();

            return services;
        }
    }

    //public sealed class OutputCacheOptionsSetup : IConfigureOptions<OutputCacheOptions>
    //{
    //    private readonly IServiceProvider _services;

    //    public OutputCacheOptionsSetup(IServiceProvider services)
    //    {
    //        _services = services;
    //    }

    //    public void Configure(OutputCacheOptions options)
    //    {
    //        options.ApplicationServices = _services;
    //    }
    //}
}