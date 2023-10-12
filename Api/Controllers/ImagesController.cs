using Api.Services;
using Api.Services.DTO;
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

            var mime = ExtensionToMime(imageData.Extension);
            return new FileStreamResult(imageData.Data, mime);
        }

        [HttpGet()]
        public IEnumerable<IItemInfo> ListItems(long? parentId, int take = 10)
        {
            return _storageService.GetItems(parentId, take);
        }

        private string ExtensionToMime(string extension)
        {
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