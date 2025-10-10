using System.Text.Json.Serialization;

namespace DataPlane.Sdk.Core.Domain.Messages;

public class DataFlowTerminateMessage : JsonLdDto
{
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }
}