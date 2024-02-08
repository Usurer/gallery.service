using Api.Exceptions;
using Api.Utils;
using EasyCaching.Core;
using Imageflow.Fluent;

namespace Api.Services
{
    public class ImageResizeService
    {
        private readonly IStorageService StorageService;
        private readonly IEasyCachingProvider EasyCachingProvider;
        private readonly ILogger<ImageResizeService> Logger;

        public ImageResizeService(
            IStorageService storageService,
            IEasyCachingProviderFactory easyCachingProviderFactory,
            ILogger<ImageResizeService> logger)
        {
            StorageService = storageService;
            EasyCachingProvider = easyCachingProviderFactory.GetCachingProvider("disk");
            Logger = logger;
        }

        // TODO: Can we include UpdatedAtDate into a cache key? Would be really nice for handling updated items
        public async Task<ImageResizeResult?> GetAsync(long id, int timestamp, int? width, int? height)
        {
            var key = $"{id}_{timestamp}_{width}_{height}";
            var cacheResult = EasyCachingProvider.Get<ImageResizeResult>(key);
            if (!cacheResult.HasValue)
            {
                using var imageData = StorageService.GetImage(id);

                if (imageData == null)
                {
                    Logger.LogWarning("Image with id {ImageId} was not found", id);
                    return null;
                }

                var widthParam = width.HasValue ? $"width={width}" : string.Empty;
                var heightParam = height.HasValue ? $"height={height}" : string.Empty;
                var resizeParam = string.Join("&", new[] { widthParam, heightParam }.Where(x => !string.IsNullOrEmpty(x)));

                MemoryStream resizedStream = new MemoryStream();
                var job = new ImageJob();
                var resizeResult = await job.Decode(imageData.Data, true)
                    .ResizerCommands($"{resizeParam}&mode=crop")
                    // TODO: Set disposeUnderlying to true?
                    .Encode(new StreamDestination(resizedStream, false), new PngQuantEncoder())
                    .Finish()
                    .InProcessAsync();

                var data = resizedStream.ToArray();
                var mime = MimeUtils.ExtensionToMime(imageData.Info.Extension);
                var result = new ImageResizeResult { Data = data, MimeType = mime };
                try
                {
                    EasyCachingProvider.Set<ImageResizeResult>(key, result, TimeSpan.FromDays(1));
                }
                catch (DirectoryNotFoundException ex)
                {
                    Logger.LogError(ex, "Cache failure, directory not found");
                    // If the cache folder was removed while the app is running, there seems to be
                    // no way to recreate it and FlushAsync I'm using here doesn't help.
                    // TODO: Find a way to re-instantiate stuff that was setup on app start
                    await EasyCachingProvider.FlushAsync();
                }

                return result;
            }

            return cacheResult.Value;
        }
    }

    public class ImageResizeResult
    {
        public required byte[] Data
        {
            get; set;
        }

        public required string MimeType
        {
            get; set;
        }
    }
}