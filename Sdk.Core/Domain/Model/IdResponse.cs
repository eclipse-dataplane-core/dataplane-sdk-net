using System.Text.Json.Serialization;
using static Sdk.Core.Domain.IConstants;

namespace Sdk.Core.Domain.Model;

public class IdResponse(string id)
{
    [field: JsonPropertyName("@id")]
    public string Id { get; init; } = id;

    [field: JsonPropertyName(EdcNamespace + "createdAt")]
    public long CreatedAt { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}