using System.Text.Json.Serialization;

namespace Sdk.Core.Domain.Messages;

public class DataFlowTerminationMessage
{
    [JsonPropertyName(IConstants.EdcNamespace + "reason")]
    public string? Reason { get; set; }
}