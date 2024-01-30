using Api.Services;

namespace Api.BackgroundServices
{
    public class ScheduledScanService : BackgroundService
    {
        private readonly IServiceProvider Services;
        private Timer? timer = null;
        private bool IsRunning;

        public ScheduledScanService(IServiceProvider services)
        {
            Services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            timer = new Timer(GetItems, null, 0, 5 * 1000);
            return;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private void GetItems(object? state)
        {
            if (IsRunning)
                return;

            IsRunning = true;
            using var scope = Services.CreateScope();
            var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
            var items = storageService.GetFolderItems(null, 0, 1);
            IsRunning = false;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}