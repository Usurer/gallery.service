using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("internals/[controller]/[action]")]
    public class FileSystemController : ControllerBase
    {
        private readonly IStorageService StorageService;
        private readonly IFileSystemService FileSystemService;

        public FileSystemController(IStorageService storageService, IFileSystemService fileSystemService)
        {
            StorageService = storageService;
            FileSystemService = fileSystemService;
        }

        [HttpGet()]
        public async Task<ScanFolderResult> Scan(string? folder)
        {
            return await FileSystemService.ScanFolderAsync(folder, false, null);
        }

        [HttpGet()]
        public IEnumerable<IFileOrFolderInfo> GetFileSystemItems(string? folder)
        {
            return StorageService.GetFileSystemItems(folder);
        }
    }
}