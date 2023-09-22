using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("internals/[controller]/[action]")]
    public class FileSystemController : ControllerBase
    {
        private readonly IFileSystemService FileSystemService;

        public FileSystemController(IFileSystemService fileSystemService)
        {
            FileSystemService = fileSystemService;
        }

        [HttpGet()]
        public IEnumerable<IFileSystemItem> Scan(string folder)
        {
            return FileSystemService.ScanFolder(folder);
        }
    }
}