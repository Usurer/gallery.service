using Api.Services;
using Api.Services.DTO;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers.Internal
{
    /// <summary>
    /// Test controller to scan FS and query DB
    /// </summary>
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
        [SwaggerOperation("Scans a folder under the given fullPath, or default folder if fullPath is not provided")]
        public async Task<ScanFolderResult> ScanFolder(string? folder)
        {
            return await FileSystemService.ScanFolderAsync(folder, null);
        }

        [HttpGet()]
        [SwaggerOperation("Scans all folders recursively, starting from root or default folder if root is not provided")]
        [SwaggerResponse(200, "ScanFolderResult entity for every scanned folder")]
        public async IAsyncEnumerable<ScanFolderResult> ScanTree(string? root)
        {
            // Doesn't seem to work
            var buffFeature = Response.HttpContext.Features.Get<IHttpResponseBodyFeature>();
            buffFeature.DisableBuffering();

            await foreach (var r in FileSystemService.ScanFoldersFromRootAsync(root))
            {
                yield return r;
            }
        }

        [HttpGet()]
        public IEnumerable<IItemInfo> GetFileSystemItems(long? rootId)
        {
            return StorageService.GetItems(rootId, 0, 500);
        }
    }
}