using System.Text.Json;
using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain.Model;

namespace Provider.Nats;

public class NatsDataAddress : DataAddress
{
    public NatsDataAddress() : base(Constants.DataAddressType)
    {
        Properties["endpointType"] = "https://example.com/natsdp/v1/nats";
    }

    public string NatsEndpoint
    {
        init => Properties["endpoint"] = value;
        get => Properties["endpoint"] as string ?? throw new InvalidOperationException("No 'endpoint' endpointProperty found");
    }

    public string Channel
    {
        get
        {
            var property = GetEndpointProperty("channel");
            return (property?.Value ?? null) ?? throw new InvalidOperationException("No 'channel' endpointProperty found");
        }
        init => StringEndpointProperty("channel", value);
    }

    public string ReplyChannel
    {
        init => StringEndpointProperty("replyChannel", value);
    }

    private EndpointProperty? GetEndpointProperty(string key)
    {
        var props = Properties["endpointProperties"];

        List<EndpointProperty> epp;
        if (props is JsonElement)
        {
            epp = JsonSerializer.Deserialize<List<EndpointProperty>>(props.ToString());
        }
        else
        {
            epp = props as List<EndpointProperty>;
        }


        var property = epp?.Find(p => p.Key.Equals(key));
        return property;
    }

    public static NatsDataAddress Create(DataAddress rawSource)
    {
        return new NatsDataAddress
        {
            Properties = rawSource.Properties,
            Id = rawSource.Id
        };
    }

    private void StringEndpointProperty(string key, string endpointPropertyValue)
    {
        if (!Properties.TryGetValue("endpointProperties", out var existing))
        {
            existing = new List<EndpointProperty>();
            Properties["endpointProperties"] = existing;
        }

        var epProps = existing as List<EndpointProperty>;
        epProps!.Add(new EndpointProperty
        {
            Key = key,
            Type = "string",
            Value = endpointPropertyValue
        });
    }

    public class EndpointProperty
    {
        [JsonPropertyName("key")]
        public required string Key { get; init; }

        [JsonPropertyName("type")]
        public required string Type { get; init; }

        [JsonPropertyName("value")]
        public required string Value { get; init; }
    }
}