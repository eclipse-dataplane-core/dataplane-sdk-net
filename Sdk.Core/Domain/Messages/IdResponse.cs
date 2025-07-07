using System.Text.Json.Serialization;
using Sdk.Core.Infrastructure;
using static Sdk.Core.Domain.IConstants;

namespace Sdk.Core.Domain.Messages;

public class IdResponse(string id) : JsonLdDto(nameof(IdResponse))
{
    [JsonPropertyName("@id")]
    public string Id { get; init; } = id;

    [JsonPropertyName(EdcNamespace + "createdAt")]
    public long CreatedAt { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}