using System.Text.Json.Serialization;
using static DataPlane.Sdk.Core.Domain.IConstants;

namespace DataPlane.Sdk.Core.Domain.Messages;

public class IdResponse(string id) : JsonLdDto(nameof(IdResponse))
{
    [JsonPropertyName("@id")]
    public string Id { get; init; } = id;

    [JsonPropertyName(EdcNamespace + "createdAt")]
    public long CreatedAt { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}