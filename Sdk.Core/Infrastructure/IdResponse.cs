using System.Text.Json.Serialization;
using static Sdk.Core.Domain.IConstants;

namespace Sdk.Core.Infrastructure;

public class IdResponse(string id) : JsonLdDto
{
    [JsonPropertyName("@id")]
    public string Id { get; init; } = id;

    [JsonPropertyName(EdcNamespace + "createdAt")]
    public long CreatedAt { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}