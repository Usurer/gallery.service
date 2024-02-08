using Api.Services;
using Api.Services.DTO;
using Api.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Controllers
{
    /// <summary>
    /// List images, get image by ID
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class ImagesController : ControllerBase
    {
        private readonly IStorageService _storageService;

        private readonly ImageResizeService _resizeService;

        public ImagesController(IStorageService storageService, ImageResizeService resizeService)
        {
            _storageService = storageService;
            _resizeService = resizeService;
        }

        [HttpGet()]
        [ResponseCache(Duration = 60)]
        public Results<NotFound, FileStreamHttpResult> GetImage([BindRequired] long id)
        {
            // imageData is disposable because of the Data stream, but FileStreamResult should take care of it
            var imageData = _storageService.GetImage(id);

            if (imageData == null)
            {
                return TypedResults.NotFound();
            }

            var mime = MimeUtils.ExtensionToMime(imageData.Info.Extension);

            return TypedResults.File(imageData.Data, mime, imageData.Info.Name);
        }

        [HttpGet()]
        [ResponseCache(Duration = 3600 * 24)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<Results<ProblemHttpResult, FileContentHttpResult>> GetImagePreview(
            [BindRequired] long id, int timestamp, int? width, int? height
        )
        {
            if (width == null && height == null)
            {
                return TypedResults.Problem(

                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Missing parameters",
                    detail: "Either width or height should be provided"
                );
            }

            var result = await _resizeService.GetAsync(id, timestamp, width, height);
            if (result == null)
            {
                return TypedResults.Problem(

                    statusCode: StatusCodes.Status404NotFound,
                    title: "Image not found"
                );
            }

            return TypedResults.File(result.Data, contentType: result.MimeType);
        }

        [HttpGet]
        [Route("{folderId}")]
        [Route("")]
        public IEnumerable<FileItemInfo> ListItems(long? folderId, int skip = 0, int take = 10, [FromQuery] string[]? extensions = null)
        {
            return _storageService.GetFileItems(folderId, skip, take, extensions);
        }
    }
}