using System.Text.Json.Serialization;

namespace Sdk.Core.Infrastructure;

public class Lease
{
    [JsonPropertyName("leasedBy")]
    public required string LeasedBy { get; init; }

    [JsonPropertyName("leasedAt")]
    public required long LeasedAt { get; init; }

    [JsonPropertyName("leaseDuration")]
    public required long LeaseDurationMillis { get; init; }

    [JsonIgnore]
    public required string Id { get; init; }

    public bool IsExpired(long? now = null)
    {
        now ??= DateTime.UtcNow.Millisecond;
        return LeasedAt + LeaseDurationMillis < now;
    }
}