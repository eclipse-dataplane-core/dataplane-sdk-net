using Sdk.Core.Domain;

namespace Sdk.Core.Test;

public static class TestMethods
{
    public static DataFlow CreateDataFlow(string id)
    {
        return new DataFlow(id)
        {
            Source = new DataAddress("test-data-address"),
            Destination = new DataAddress("test-data-address"),
            TransferType = new TransferType("test-type", FlowType.Pull),
            RuntimeId = "test-runtime",
            ParticipantId = "test-participant",
            AssetId = "test-asset",
            AgreementId = "test-agreement"
        };
    }
}