using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Messages;

/// <summary>
///     Represents a data flow start message from the Dataplane Signaling API protocol. It is used to initiate a data
///     transfer between a consumer and the provider. This message is sent by the control plane to the data plane.
/// </summary>
public class DataFlowStartedNotificationMessage : JsonLdDto
{
    [JsonPropertyName("dataAddress")]
    public DataAddress? DataAddress { get; init; }
}