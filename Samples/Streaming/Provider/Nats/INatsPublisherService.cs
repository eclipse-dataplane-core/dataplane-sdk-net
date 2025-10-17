namespace Provider.Nats;

public interface INatsPublisherService
{
    void Start(string channel);
    Task StopAsync(string channel);
}