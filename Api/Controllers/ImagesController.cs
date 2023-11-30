using Api.Attributes;
using Api.Services;
using Api.Services.DTO;
using Api.Utils;
using EasyCaching.Core;
using Imageflow.Fluent;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Diagnostics;

namespace Api.Controllers
{
    /// <summary>
    /// List images, get image by ID
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class ImagesController : ControllerBase
    {
        // TODO: We should not use DB context here directly, but get all data via IStorageService
        private readonly IStorageService _storageService;

        public ImagesController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet()]
        [ResponseCache(Duration = 60)]
        public IActionResult GetImage(long id)
        {
            // imageData is disposable because of the Data stream, but FileStreamResult should take care of it
            var imageData = _storageService.GetImage(id);

            if (imageData == null)
            {
                return new EmptyResult();
            }

            var mime = MimeUtils.ExtensionToMime(imageData.Info.Extension);
            return new FileStreamResult(imageData.Data, mime);
        }

        [HttpGet()]
        // TODO: Add this feature to the ImageCaching Middleware
        //[ResponseCache(Duration = 60)]
        //[ImageCache(DurationMinutes = 60)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetImagePreview(long id, int? width, int? height)
        {
            if (width == null && height == null)
            {
                return new ObjectResult(
                    new
                    {
                        Message = "Either width or height should be provided"
                    })
                {
                    StatusCode = StatusCodes.Status422UnprocessableEntity
                };
            }

            using var imageData = _storageService.GetImage(id);

            if (imageData == null)
            {
                return new EmptyResult();
            }

            var widthParam = width.HasValue ? $"width={width}" : string.Empty;
            var heightParam = height.HasValue ? $"height={height}" : string.Empty;
            var resizeParam = string.Join("&", new[] { widthParam, heightParam }.Where(x => !string.IsNullOrEmpty(x)));

            MemoryStream resizedStream = new MemoryStream();
            var job = new ImageJob();
            var result = await job.Decode(imageData.Data, true)
                .ResizerCommands($"{resizeParam}&mode=crop")
                .Encode(new StreamDestination(resizedStream, false), new PngQuantEncoder())
                .Finish()
                .InProcessAsync();

            resizedStream.Position = 0;

            var mime = MimeUtils.ExtensionToMime(imageData.Info.Extension);
            return new FileStreamResult(resizedStream, result.First.PreferredMimeType);
        }

        [HttpGet()]
        public IEnumerable<IItemInfo> ListItems(long? parentId, int skip = 0, int take = 10)
        {
            /* TODO: Instead of IItemInfo[] return two arays: one of files, another of folders
             * This will allow to get rid of IItemInfo which doesn't have any value
             * And the whole thing would be better typed, because folders have no dimensions
             */
            return _storageService.GetItems(parentId, skip, take);
        }
    }
}