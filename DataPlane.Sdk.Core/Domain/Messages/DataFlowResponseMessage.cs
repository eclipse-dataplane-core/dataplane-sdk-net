using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Messages;

public class DataFlowResponseMessage : JsonLdDto
{
    [JsonPropertyName(IConstants.EdcNamespace + "dataAddress")]
    public required DataAddress DataAddress { get; set; }

    [JsonPropertyName(IConstants.EdcNamespace + "provisioning")]
    public bool IsProvisioned { get; set; } = false;
}