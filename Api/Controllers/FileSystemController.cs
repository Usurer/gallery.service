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
        public async Task<ScanFolderResult> ScanFolder(string? folder)
        {
            return await FileSystemService.ScanFolderAsync(folder, null);
        }

        [HttpGet()]
        public async Task<IList<ScanFolderResult>> ScanTree(string? root)
        {
            return await FileSystemService.ScanTreeAsync(root);
        }

        [HttpGet()]
        public IEnumerable<IFileOrFolderInfo> GetFileSystemItems(string? folder)
        {
            return StorageService.GetFileSystemItems(folder, 500);
        }
    }
}