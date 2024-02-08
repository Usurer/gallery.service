using Api.Services;
using Api.Services.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class MetaController : ControllerBase
    {
        private IStorageService _storageService;

        public MetaController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet]
        public IResult GetImagesMetadata(long? parentId)
        {
            var result = _storageService.GetCollectionMetadata(parentId);
            return TypedResults.Ok(result);
        }

        [HttpGet]
        public Results<ProblemHttpResult, Ok<ItemInfo>> GetItemMetadata([BindRequired] long id)
        {
            var result = _storageService.GetItem(id);
            if (result == null)
            {
                return TypedResults.Problem(
                    title: "Item not found",
                    detail: "The requested item doesn't exist",
                    statusCode: StatusCodes.Status404NotFound
                );
            }
            return TypedResults.Ok(result);
        }
    }
}