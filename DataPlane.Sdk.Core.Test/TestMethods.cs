using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Test;

public static class TestMethods
{
    public static DataFlow CreateDataFlow(string id, DataFlowState state = 0)
    {
        return new DataFlow(id)
        {
            Source = new DataAddress("test-data-address"),
            Destination = new DataAddress("test-data-address")
            {
                Properties = { ["test-key"] = "test-value" }
            },
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            },
            RuntimeId = "test-runtime",
            ParticipantId = "test-participant",
            AssetId = "test-asset",
            AgreementId = "test-agreement",
            State = state
        };
    }

    public static DataFlowStartMessage CreateStartMessage()
    {
        return new DataFlowStartMessage
        {
            ProcessId = "test-process-id",
            SourceDataAddress = new DataAddress("test-source-type"),
            DestinationDataAddress = new DataAddress("test-destination-type"),
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            },
            ParticipantId = "test-participant-id",
            DatasetId = "test-asset-id",
            AgreementId = "test-agreement-id"
        };
    }

    public static DataFlowPrepareMessage CreatePrepareMessage()
    {
        return new DataFlowPrepareMessage
        {
            ProcessId = "test-process-id",
            DestinationDataAddress = new DataAddress("test-destination-type"),
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Pull
            },
            ParticipantId = "test-participant-id",
            DatasetId = "test-asset-id",
            AgreementId = "test-agreement-id"
        };
    }
}