using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
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
    }
}