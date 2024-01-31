using Api.Services;
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
        public async Task<ActionResult> AddScan(string path)
        {
            var id = await ScansStateService.AddFolderToScansAsync(path);
            _ = ScansProcessingService.EnqueueNextScanAsync(id);
            return new OkResult();
        }
    }
}