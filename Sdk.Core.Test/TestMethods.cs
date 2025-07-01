using Sdk.Core.Domain;

namespace Sdk.Core.Test;

public class TestMethods
{
    public static DataFlow CreateDataFlow(string id)
    {
        return new DataFlow(id)
        {
            Source = new DataAddress(),
            Destination = new DataAddress(),
            TransferType = new TransferType("test-type", FlowType.Pull),
            RuntimeId = "test-runtime",
            ParticipantId = "test-participant",
            AssetId = "test-asset",
            AgreementId = "test-agreement"
        };
    }
}