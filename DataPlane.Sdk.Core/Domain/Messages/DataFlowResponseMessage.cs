using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Messages;

public class DataFlowResponseMessage : JsonLdDto
{
    [JsonPropertyName("dataAddress")]
    public DataAddress? DataAddress { get; init; }

    [JsonPropertyName("dataplaneID")]
    public required string DataplaneId { get; init; }

    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required DataFlowState State { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("error")]
    public string? Error { get; init; }
}