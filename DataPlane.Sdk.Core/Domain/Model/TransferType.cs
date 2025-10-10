using System.Text.Json.Serialization;

namespace DataPlane.Sdk.Core.Domain.Model;

/// <summary>
///     Represents a transfer type.
/// </summary>
/// <param name="DestinationType">The physical location where data is supposed to go</param>
/// <param name="FlowType">push or pull</param>
/// <param name="ResponseChannel">optional: the type designation for the response channel</param>
public class TransferType
{
    [JsonPropertyName("destinationType")]
    public required string DestinationType { get; init; }

    [JsonPropertyName("flowType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FlowType FlowType { get; init; }

    /// <summary>optional: the type designation for the response channel</summary>
    public string? ResponseChannel { get; init; }
}

public enum FlowType
{
    Push,
    Pull
}