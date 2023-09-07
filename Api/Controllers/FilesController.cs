using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class FilesController : ControllerBase
    {
        [HttpGet()]
        public IActionResult GetImage()
        {
            // Here we can - and should - ignore `using`. Adding it will throw an Object Disposed exception
            var image = new FileStream("C:\\Users\\Usurer.000\\Pictures\\DSC06344.JPG", FileMode.Open);
            return File(image, "image/jpeg");
        }

        [HttpGet()]
        public IActionResult GetArray()
        {
            // Mind that missing `using` here will result with the file being locked
            using var image = new FileStream("C:\\Users\\Usurer.000\\Pictures\\DSC06344.JPG", FileMode.Open);
            var bytes = new byte[image.Length];

            image.Read(bytes);

            return Ok(new ImageData { Bytes = bytes.ToArray() });
        }

        [HttpGet()]
        public async Task GetBytes()
        {
            using var image = new FileStream("C:\\Users\\Usurer.000\\Pictures\\DSC06344.JPG", FileMode.Open);
            var bytes = new byte[image.Length];

            image.Read(bytes);

            var imgSize = BitConverter.GetBytes(image.Length);

            for (int i = 0; i < 10; i++)
            {
                var writer = new StreamWriter(Response.Body);
                await Response.Body.WriteAsync(imgSize, 0, 4);
                await Response.Body.WriteAsync(bytes, 0, bytes.Length);
                await Response.Body.FlushAsync();

                await Task.Delay(1000 * 1);
            }

            return;
        }
    }

    public class ImageData
    {
        public byte[] Bytes
        {
            get; set;
        }

        //public long Length
        //{
        //    get; set;
        //}
    }
}