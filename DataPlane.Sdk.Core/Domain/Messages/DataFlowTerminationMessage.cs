using System.Text.Json.Serialization;

namespace DataPlane.Sdk.Core.Domain.Messages;

public class DataFlowTerminationMessage : JsonLdDto
{
    [JsonPropertyName(IConstants.EdcNamespace + "reason")]
    public string? Reason { get; set; }
}