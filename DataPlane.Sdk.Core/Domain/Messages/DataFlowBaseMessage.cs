using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Messages;

public abstract class DataFlowBaseMessage : JsonLdDto
{
    [JsonPropertyName("messageID")]
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("participantId")]
    public required string ParticipantId { get; init; }

    [JsonPropertyName("counterPartyID")]
    public string? CounterPartyId { get; set; }

    [JsonPropertyName("dataspaceContext")]
    public string? DataspaceContext { get; set; }

    [JsonPropertyName("processId")]
    public required string ProcessId { get; set; }

    [JsonPropertyName("agreementId")]
    public required string AgreementId { get; init; }

    [JsonPropertyName("datasetId")]
    public required string DatasetId { get; init; }

    [JsonPropertyName("callbackAddress")]
    public Uri? CallbackAddress { get; set; }

    [JsonPropertyName("transferType")]
    public required string TransferType { get; init; }

    [JsonPropertyName("destinationDataAddress")]
    public required DataAddress DestinationDataAddress { get; init; }
}