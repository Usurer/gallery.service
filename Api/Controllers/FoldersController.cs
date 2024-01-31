using Api.Services;
using Api.Services.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class FoldersController : ControllerBase
    {
        private readonly IStorageService _storageService;

        public FoldersController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet]
        [Route("{parentId}")]
        [Route("")]
        public IEnumerable<FolderItemInfo> ListItems(long? parentId, int skip = 0, int take = 10)
        {
            return _storageService.GetFolderItems(parentId, skip, take);
        }

        [HttpGet]
        [Route("{folderId}")]
        [Route("")]
        public IEnumerable<FolderItemInfo> GetAncestors(long folderId)
        {
            return _storageService.GetFolderAncestors(folderId);
        }
    }
}