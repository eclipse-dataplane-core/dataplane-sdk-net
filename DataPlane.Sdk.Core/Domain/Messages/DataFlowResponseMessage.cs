using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Messages;

public class DataFlowResponseMessage : JsonLdDto
{
    [JsonPropertyName("dataAddress")]
    public DataAddress? DataAddress { get; init; }

    [JsonPropertyName("dataplaneId")]
    public required string DataplaneId { get; init; }

    [JsonPropertyName("state")]
    public string? State { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }
}