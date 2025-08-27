using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Domain.Messages;

public class DataFlowPrepareMessage
{
    [JsonPropertyName("processId")]
    public required string ProcessId { get; set; }

    [JsonPropertyName("datasetId")]
    public required string AssetId { get; set; }

    [JsonPropertyName("participantId")]
    public required string ParticipantId { get; set; }

    [JsonPropertyName("agreementId")]
    public required string AgreementId { get; set; }

    [JsonPropertyName("sourceDataAddress")]
    public required DataAddress SourceDataAddress { get; set; }

    [JsonPropertyName("destinationDataAddress")]
    public required DataAddress DestinationDataAddress { get; set; }

    [JsonPropertyName("callbackAddress")]
    public Uri? CallbackAddress { get; set; }

    [JsonPropertyName("properties")]
    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();

    [JsonPropertyName("flowType")]
    public required string TransferType { get; init; }

    [JsonPropertyName("transferTypeDestination")]
    public required string TransferTypeDestination { get; init; }
}