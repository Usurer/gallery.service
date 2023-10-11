using Api.Database;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Internal
{
    /// <summary>
    /// Test controller for direct access to the DbContext
    /// </summary>
    [ApiController]
    [Route("internals/[controller]/[action]")]
    public class DatabaseController : ControllerBase
    {
        private readonly ILogger<DatabaseController> _logger;
        private readonly GalleryContext _context;

        public DatabaseController(ILogger<DatabaseController> logger, GalleryContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IEnumerable<FileSystemItem> Get()
        {
            var item = _context.FileSystemItems;
            return item;
        }

        [HttpPut]
        public ActionResult Put()
        {
            var item = new FileSystemItem
            {
                Path = "Some path",
            };
            _context.FileSystemItems.Add(item);
            _context.SaveChanges();

            return Ok();
        }
    }
}