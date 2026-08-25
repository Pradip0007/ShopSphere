using System.Net.Http.Json;

namespace ShopSphere.Workers.Jobs;

public sealed class DeadLetterMonitorJob : BackgroundService
{
    private static readonly TimeSpan Period = TimeSpan.FromMinutes(1);

    private readonly HttpClient _http;
    private readonly ILogger<DeadLetterMonitorJob> _logger;

    public DeadLetterMonitorJob(
        IHttpClientFactory httpClientFactory,
        ILogger<DeadLetterMonitorJob> logger)
    {
        _http = httpClientFactory.CreateClient("RabbitManagement");
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Period);

        do
        {
            try
            {
                await CheckAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DLQ check failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CheckAsync(CancellationToken ct)
    {
        var queues =
            await _http.GetFromJsonAsync<List<QueueDto>>(
                "api/queues",
                ct)
            ?? new List<QueueDto>();

        var errorQueues = queues
            .Where(q => q.Name.EndsWith(
                "_error",
                StringComparison.Ordinal))
            .Where(q => q.Messages > 0)
            .ToList();

        if (errorQueues.Count == 0)
        {
            _logger.LogDebug("No dead-lettered messages.");
            return;
        }

        foreach (var q in errorQueues)
        {
            _logger.LogWarning(
                "Dead-letter queue has messages | queue={Queue} messages={Messages}",
                q.Name,
                q.Messages);
        }
    }

    private sealed record QueueDto(
        string Name,
        long Messages);
}