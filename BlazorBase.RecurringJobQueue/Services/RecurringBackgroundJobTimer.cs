using Microsoft.Extensions.Hosting;

namespace BlazorBase.RecurringBackgroundJobQueue.Services;

public class RecurringBackgroundJobTimer(RecurringBackgroundJobQueue recurringBackgroundJobQueue) : IHostedService, IDisposable
{
    private readonly RecurringBackgroundJobQueue RecurringBackgroundJobQueue = recurringBackgroundJobQueue;
    private Timer? Timer = null;

    public Task StartAsync(CancellationToken stoppingToken)
    {
        Timer = new Timer(async (state) => await RecurringBackgroundJobQueue.TriggerBackgroundJobsAsync().ConfigureAwait(false),
            null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        Timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Timer?.Dispose();
    }
}
