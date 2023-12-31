﻿using Api.Services;
using Api.Services.DTO;
using Api.Utils;
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
        private readonly IStorageService _storageService;

        private readonly ImageResizeService _resizeService;

        public ImagesController(IStorageService storageService, ImageResizeService resizeService)
        {
            _storageService = storageService;
            _resizeService = resizeService;
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
        [ResponseCache(Duration = 3600 * 24)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IResult> GetImagePreview(long id, int? width, int? height)
        {
            if (width == null && height == null)
            {
                return Results.Json(
                    new
                    {
                        Message = "Either width or height should be provided"
                    },
                    statusCode: StatusCodes.Status422UnprocessableEntity
                );
            }

            var result = await _resizeService.GetAsync(id, width, height);
            if (result == null)
            {
                return Results.NoContent();
            }

            return Results.Bytes(result.Data, contentType: result.MimeType);
        }

        [HttpGet()]
        public IEnumerable<IItemInfo> ListItems(long? parentId, int skip = 0, int take = 10, [FromQuery] string[]? extensions = null)
        {
            /* TODO: Instead of IItemInfo[] return two arays: one of files, another of folders
             * This will allow to get rid of IItemInfo which doesn't have any value
             * And the whole thing would be better typed, because folders have no dimensions
             */
            return _storageService.GetItems(parentId, skip, take, extensions);
        }
    }
}