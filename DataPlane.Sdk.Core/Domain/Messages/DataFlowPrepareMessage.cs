using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Messages;

public class DataFlowPrepareMessage : DataFlowBaseMessage
{
    [JsonPropertyName("sourceDataAddress")]
    public required DataAddress SourceDataAddress { get; set; }

    [JsonPropertyName("properties")]
    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();
}