using System.Text.Json.Serialization;

namespace Sdk.Core.Infrastructure;

public class Lease(string leasedBy, long leasedAt, long leaseDurationMillis)
{
    [JsonPropertyName("leasedBy")]
    public string LeasedBy { get; } = leasedBy;

    [JsonPropertyName("leasedAt")]
    public long LeasedAt { get; } = leasedAt;

    [JsonPropertyName("leaseDuration")]
    public long LeaseDurationMillis { get; } = leaseDurationMillis;

    public bool IsExpired(long now)
    {
        return LeasedAt + LeaseDurationMillis < now;
    }
}