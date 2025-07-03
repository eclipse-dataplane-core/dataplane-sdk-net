using System.Text.Json.Serialization;
using Sdk.Core.Domain.Model;

namespace Sdk.Core.Domain.Messages;

/// <summary>
///     Represents a data flow start message from the Dataplane Signaling API protocol. It is used to initiate a data
///     transfer
///     between a consumer and the provider. This message is sent by the control plane to the data plane.
/// </summary>
public class DataflowStartMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName(IConstants.EdcNamespace + "processId")]
    public required string ProcessId { get; set; }

    [JsonPropertyName(IConstants.EdcNamespace + "datasetId")]
    public required string AssetId { get; set; }

    [JsonPropertyName(IConstants.EdcNamespace + "participantId")]
    public required string ParticipantId { get; set; }

    [JsonPropertyName(IConstants.EdcNamespace + "agreementId")]
    public required string AgreementId { get; set; }

    [JsonPropertyName(IConstants.EdcNamespace + "sourceDataAddress")]
    public required DataAddress SourceDataAddress { get; set; }

    [JsonPropertyName(IConstants.EdcNamespace + "destinationDataAddress")]
    public required DataAddress DestinationDataAddress { get; set; }

    [JsonPropertyName(IConstants.EdcNamespace + "callbackAddress")]
    public Uri? CallbackAddress { get; set; }

    [JsonPropertyName(IConstants.EdcNamespace + "properties")]
    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();

    [JsonPropertyName(IConstants.EdcNamespace + "flowType")]
    public required TransferType TransferType { get; set; }
}