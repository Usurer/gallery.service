using Api.Database;
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
        private readonly GalleryContext _context;

        public ImagesController(GalleryContext context)
        {
            _context = context;
        }

        [HttpGet()]
        public IActionResult Get(int id)
        {
            var fileItem = _context.FileSystemItems.SingleOrDefault(x => x.Id == id && x.IsFolder == false);

            if (fileItem == null)
            {
                return new EmptyResult();
            }

            var image = new FileStream(fileItem.Path, FileMode.Open);
            var mime = ExtensionToMime(fileItem.Extension);
            //return File(image, mime);
            return new FileStreamResult(image, mime);
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