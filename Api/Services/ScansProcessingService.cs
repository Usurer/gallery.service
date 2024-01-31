using Api.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Api.Services
{
    public interface IScansProcessingService
    {
        public Task EnqueueNextScanAsync(long scanTargetId);

        public Task RunScanningAsync();
    }

    public class ScansProcessingService : IScansProcessingService
    {
        private ConcurrentQueue<long> queue = new ConcurrentQueue<long>();
        private readonly IServiceProvider ServiceProvider;
        private long RunningScanLock = 0;

        public ScansProcessingService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public async Task EnqueueNextScanAsync(long scanTargetId)
        {
            queue.Enqueue(scanTargetId);
            if (Interlocked.Read(ref RunningScanLock) == 0)
            {
                await RunScanningAsync();
            }
        }

        public async Task RunScanningAsync()
        {
            if (Interlocked.Exchange(ref RunningScanLock, 1) == 1)
            {
                return;
            }
            while (queue.TryDequeue(out var id))
            {
                var scope = ServiceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<GalleryContext>();
                var fileSystemService = scope.ServiceProvider.GetRequiredService<IFileSystemService>();

                try
                {
                    var item = await dbContext.ScanTargets.Where(x => x.Id == id).AsTracking().SingleAsync();
                    _ = await fileSystemService.ScanFoldersFromRootAsync(item.Path).ToArrayAsync();

                    dbContext.ScanTargets.Remove(item);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception)
                {
                    // TODO: Log
                    throw;
                }
            }
            Interlocked.Exchange(ref RunningScanLock, 0);
        }
    }
}