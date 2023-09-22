using Api.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly GalleryContext _context;

        public DatabaseController(ILogger<WeatherForecastController> logger, GalleryContext context)
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