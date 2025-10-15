using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Messages;

public abstract class DataFlowBaseMessage : JsonLdDto
{
    [JsonPropertyName("messageID")]
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("participantID")]
    public required string ParticipantId { get; init; }

    [JsonPropertyName("counterPartyID")]
    public string? CounterPartyId { get; set; }

    [JsonPropertyName("dataspaceContext")]
    public string? DataspaceContext { get; set; }

    [JsonPropertyName("processID")]
    public required string ProcessId { get; set; }

    [JsonPropertyName("agreementID")]
    public required string AgreementId { get; init; }

    [JsonPropertyName("datasetID")]
    public required string DatasetId { get; init; }

    [JsonPropertyName("callbackAddress")]
    public Uri? CallbackAddress { get; set; }

    [JsonPropertyName("transferType")]
    public required TransferType TransferType { get; init; }

    [JsonPropertyName("dataAddress")]
    public DataAddress? DataAddress { get; init; }
}