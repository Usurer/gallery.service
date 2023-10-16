using Api.Services;
using Api.Services.DTO;
using Api.Utils;
using Imageflow.Fluent;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetImagePreview(long id, int width)
        {
            using var imageData = _storageService.GetImage(id);

            if (imageData == null)
            {
                return new EmptyResult();
            }

            Stream resizedStream = new MemoryStream();
            var job = new ImageJob();
            var result = await job.Decode(imageData.Data, true)
                .ResizerCommands($"width={width}&mode=crop")
                .Encode(new StreamDestination(resizedStream, false), new PngQuantEncoder())
                .Finish()
                .InProcessAsync();

            resizedStream.Position = 0;

            var mime = MimeUtils.ExtensionToMime(imageData.Info.Extension);
            return new FileStreamResult(resizedStream, result.First.PreferredMimeType);
        }

        [HttpGet()]
        public IEnumerable<IItemInfo> ListItems(long? parentId, int take = 10)
        {
            return _storageService.GetItems(parentId, take);
        }
    }
}