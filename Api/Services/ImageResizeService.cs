using Api.Utils;
using EasyCaching.Core;
using Imageflow.Fluent;

namespace Api.Services
{
    public class ImageResizeService
    {
        private readonly IStorageService _storageService;
        private readonly IEasyCachingProvider _easyCachingProvider;

        public ImageResizeService(IStorageService storageService, IEasyCachingProviderFactory easyCachingProviderFactory)
        {
            _storageService = storageService;
            _easyCachingProvider = easyCachingProviderFactory.GetCachingProvider("disk");
        }

        public async Task<ImageResizeResult?> GetAsync(long id, int? width, int? height)
        {
            var key = $"{id}_{width}_{height}";
            var cacheResult = _easyCachingProvider.Get<ImageResizeResult>(key);
            if (!cacheResult.HasValue)
            {
                using var imageData = _storageService.GetImage(id);

                if (imageData == null)
                {
                    // TODO: Add logging
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
                _easyCachingProvider.Set<ImageResizeResult>(key, result, TimeSpan.FromDays(1));

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