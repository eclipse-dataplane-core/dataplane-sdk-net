using NATS.Client.Core;

namespace Consumer.Nats;

public class NatsSubscriber(ILogger<NatsSubscriber> logger)
{
    private static readonly IDictionary<string, Task> BackgroundTasks = new Dictionary<string, Task>();
    private static readonly IDictionary<string, CancellationTokenSource> CancellationTokens = new Dictionary<string, CancellationTokenSource>();

    public async Task Start(NatsDataAddress nats)
    {
        var channel = nats.Channel;
        var cts = new CancellationTokenSource();

        BackgroundTasks.Add(channel, Task.Run(async () =>
        {
            await using var conn = new NatsConnection(new NatsOpts
            {
                Url = nats.NatsEndpoint
            });

            while (!cts.Token.IsCancellationRequested)
            {
                await foreach (var mesg in conn.SubscribeAsync<string>(channel, cancellationToken: cts.Token))
                {
                    var data = mesg.Data;
                    logger.LogInformation("Received {Data}", data);
                }
            }
        }, cts.Token));

        CancellationTokens.Add(channel, cts);
    }


    public async Task Stop(NatsDataAddress nats)
    {
        var channel = nats.Channel;

        if (CancellationTokens.TryGetValue(channel, out var ct))
        {
            await ct.CancelAsync();
            logger.LogDebug("Stopping {Channel}", channel);
            CancellationTokens.Remove(channel);
        }

        if (BackgroundTasks.TryGetValue(channel, out var task))
        {
            await task;
            BackgroundTasks.Remove(channel);
            logger.LogDebug("Stopped {Channel}", channel);
        }
    }
}