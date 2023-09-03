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
        public IEnumerable<Image> Get()
        {
            var img = _context.Images;
            return img;
        }

        [HttpPut]
        public ActionResult Put()
        {
            var img = new Image
            {
                Path = "Some path",
            };
            _context.Images.Add(img);
            _context.SaveChanges();

            return Ok();
        }
    }
}