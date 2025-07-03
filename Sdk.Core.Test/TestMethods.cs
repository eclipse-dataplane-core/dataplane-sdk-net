using Sdk.Core.Domain.Messages;
using Sdk.Core.Domain.Model;

namespace Sdk.Core.Test;

public static class TestMethods
{
    public static DataFlow CreateDataFlow(string id, DataFlowState state = 0)
    {
        return new DataFlow(id)
        {
            Source = new DataAddress("test-data-address"),
            Destination = new DataAddress("test-data-address"),
            TransferType = new TransferType("test-type", FlowType.Pull),
            RuntimeId = "test-runtime",
            ParticipantId = "test-participant",
            AssetId = "test-asset",
            AgreementId = "test-agreement",
            State = state
        };
    }

    public static DataflowStartMessage CreateStartMessage()
    {
        var message = new DataflowStartMessage
        {
            ProcessId = "test-process-id",
            SourceDataAddress = new DataAddress("test-source-type"),
            DestinationDataAddress = new DataAddress("test-destination-type"),
            TransferType = new TransferType("test-destination-type",
                FlowType.Pull),
            ParticipantId = "test-participant-id",
            AssetId = "test-asset-id",
            AgreementId = "test-agreement-id"
        };
        return message;
    }
}