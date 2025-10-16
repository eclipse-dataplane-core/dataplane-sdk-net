namespace Provider.Nats;

public interface INatsPublisherService
{
    void StartAsync(string channel);
    Task StopAsync(string channel);
}