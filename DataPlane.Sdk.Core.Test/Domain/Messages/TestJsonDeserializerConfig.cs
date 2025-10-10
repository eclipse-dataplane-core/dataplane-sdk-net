using System.Text.Json;
using DataPlane.Sdk.Core.Data;

namespace DataPlane.Sdk.Core.Test.Domain.Messages;

public static class TestJsonDeserializerConfig
{
    public static JsonSerializerOptions DefaultOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new ObjectAsPrimitiveConverter() }
    };
}