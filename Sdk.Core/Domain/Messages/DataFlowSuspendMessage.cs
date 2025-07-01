using System.Text.Json.Serialization;

namespace Sdk.Core.Domain.Messages;

public class DataFlowSuspendMessage
{
    [JsonPropertyName(IConstants.EdcNamespace + "reason")]
    public string? Reason { get; set; }
}