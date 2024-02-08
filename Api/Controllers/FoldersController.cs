using Api.Services;
using Api.Services.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
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
        public Results<Ok<IEnumerable<FolderItemInfo>>, ProblemHttpResult> GetAncestors(long folderId)
        {
            var result = _storageService.GetFolderAncestors(folderId);
            if (result == null)
            {
                return TypedResults.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Folder not found"
                );
            }

            return TypedResults.Ok(result);
        }
    }
}