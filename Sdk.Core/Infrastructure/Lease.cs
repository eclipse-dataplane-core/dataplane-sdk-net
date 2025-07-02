using System.Text.Json.Serialization;

namespace Sdk.Core.Infrastructure;

public class Lease
{
    [JsonPropertyName("leasedBy")]
    public required string LeasedBy { get; init; }

    [JsonPropertyName("leasedAt")]
    public long LeasedAt { get; init; } = DateTime.UtcNow.Millisecond;

    [JsonPropertyName("leaseDuration")]
    public required long LeaseDurationMillis { get; init; }

    [JsonIgnore]
    public required string EntityId { get; init; }

    public bool IsExpired(long? now = null)
    {
        now ??= DateTime.UtcNow.Millisecond;
        return LeasedAt + LeaseDurationMillis < now;
    }
}