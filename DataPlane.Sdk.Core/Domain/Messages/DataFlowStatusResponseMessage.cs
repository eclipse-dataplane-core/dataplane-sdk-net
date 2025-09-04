using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Messages;

public class DataFlowStatusResponseMessage : JsonLdDto
{
    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DataFlowState State { get; set; }

    [JsonPropertyName("dataFlowId")]
    public required string Id { get; set; }
}