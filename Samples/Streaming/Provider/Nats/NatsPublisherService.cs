using System.Text.Json;
using Microsoft.Extensions.Options;
using NATS.Client.Core;

namespace Provider.Nats;

public class NatsPublisherService(ILogger<NatsPublisherService> logger, IOptions<NatsOptions> options) : INatsPublisherService
{
    private static readonly IDictionary<string, Task> BackgroundTasks = new Dictionary<string, Task>();
    private static readonly IDictionary<string, CancellationTokenSource> CancellationTokenSource = new Dictionary<string, CancellationTokenSource>();

    public void Start(string channel)
    {
        var ct = new CancellationTokenSource();
        CancellationTokenSource.Add(channel, ct);

        BackgroundTasks.Add(channel, Task.Run(async () =>
        {
            await using var nats = new NatsConnection(new NatsOpts
            {
                Url = options.Value.NatsEndpoint
            });

            var num = 0;
            while (!ct.Token.IsCancellationRequested)
            {
                try
                {
                    var eventData = new { data = $"Event {num++}", num };
                    var json = JsonSerializer.Serialize(eventData);
                    await nats.PublishAsync(channel, json, cancellationToken: ct.Token);
                    logger.LogInformation("Publish {Json}", json);
                    await Task.Delay(2000, ct.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error publishing to NATS");
                }
            }
        }, ct.Token));
    }

    public async Task StopAsync(string channel)
    {
        if (CancellationTokenSource.TryGetValue(channel, out var cts))
        {
            logger.LogDebug("Stopping {Channel}", channel);
            await cts.CancelAsync();
            CancellationTokenSource.Remove(channel);
        }

        if (BackgroundTasks.TryGetValue(channel, out var task))
        {
            await task;
            logger.LogDebug("Stopped {Channel}", channel);
            BackgroundTasks.Remove(channel);
        }
    }
}