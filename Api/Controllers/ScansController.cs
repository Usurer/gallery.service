using Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ScansController : ControllerBase
    {
        private readonly IScansStateService ScansStateService;
        private readonly IScansProcessingService ScansProcessingService;

        public ScansController(IScansStateService scansStateService, IScansProcessingService scansProcessingService)
        {
            ScansStateService = scansStateService;
            ScansProcessingService = scansProcessingService;
        }

        [HttpPut]
        public async Task<Ok> AddScan([FromBody] string path)
        {
            var id = await ScansStateService.AddFolderToScansAsync(path);

            // TODO: Figure out the proper way to do error handling here
            _ = ScansProcessingService.EnqueueNextScanAsync(id);
            return TypedResults.Ok();
        }
    }
}