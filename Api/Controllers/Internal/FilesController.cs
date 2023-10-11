using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Internal
{
    /// <summary>
    /// Test controller to experiment with returning images in response
    /// </summary>
    [ApiController]
    [Route("internals/[controller]/[action]")]
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
            var files = Directory.EnumerateFiles(@"C:\Users\Usurer.000\Pictures");
            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                var isImg = fi.Extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase)
                    || fi.Extension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase);

                if (isImg)
                {
                    using var image = new FileStream(file, FileMode.Open);
                    var bytes = new byte[image.Length];

                    image.Read(bytes);

                    var imgSize = BitConverter.GetBytes(image.Length);

                    var writer = new StreamWriter(Response.Body);
                    await Response.Body.WriteAsync(imgSize, 0, 4);
                    await Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    await Response.Body.FlushAsync();

                    await Task.Delay(1000 * 1);
                }
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