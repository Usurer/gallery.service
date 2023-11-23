using Api.Services;
using Api.Services.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MetaController : ControllerBase
    {
        private IStorageService _storageService;

        public MetaController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet]
        public IActionResult GetImagesMetadata(long? parentId)
        {
            var result = _storageService.GetCollectionMetadata(parentId);
            return Ok(result);
        }

        [HttpGet]
        public IItemInfo GetItemMetadata(long id)
        {
            var result = _storageService.GetItem(id);
            return result;
        }
    }
}