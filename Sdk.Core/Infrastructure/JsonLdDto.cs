using System.Text.Json.Serialization;
using Sdk.Core.Domain;

namespace Sdk.Core.Infrastructure;

public class JsonLdDto
{
    protected JsonLdDto()
    {
        Type = IConstants.EdcNamespace + GetType().Name;
    }

    public JsonLdDto(string type)
    {
        Type = IConstants.EdcNamespace + type;
    }

    [field: JsonPropertyName("@context")]
    public Dictionary<string, string> Context { get; } = new()
    {
        { "edc", "https://w3id.org/edc/v0.0.1/ns/" }
    };

    [JsonPropertyName("@type")]
    public string Type { get; }
}