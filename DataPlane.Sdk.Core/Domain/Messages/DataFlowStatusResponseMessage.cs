using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Messages;

public class DataFlowStatusResponseMessage : JsonLdDto
{
    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonNumberEnumConverter<DataFlowState>))]
    public DataFlowState State { get; set; }

    [JsonPropertyName("dataFlowID")]
    public required string DataFlowId { get; set; }
}