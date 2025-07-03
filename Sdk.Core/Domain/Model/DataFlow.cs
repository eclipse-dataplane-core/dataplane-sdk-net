namespace Sdk.Core.Domain.Model;

public class DataFlow(string id) : StatefulEntity<DataFlowState>(id)
{
    public required DataAddress Source { get; set; }
    public required DataAddress Destination { get; set; }
    public Uri? CallbackAddress { get; set; }
    public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

    public required TransferType TransferType { get; set; }

    // public IList<ResourceDefinition> ResourceDefinitions { get; set; }
    public required string RuntimeId { get; set; }
    public bool IsProvisionComplete { get; set; } = true;
    public bool IsProvisionRequested { get; set; } = false;
    public bool IsDeprovisionComplete { get; set; } = false;
    public bool IsDeprovisionRequested { get; set; } = false;
    public bool IsConsumer { get; set; } = false;
    public required string ParticipantId { get; set; }
    public required string AssetId { get; set; }
    public required string AgreementId { get; set; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    public void Deprovision()
    {
        Transition(DataFlowState.Deprovisioning);
    }

    public void Terminate()
    {
        Transition(DataFlowState.Terminated);
    }

    public void Suspend(string? reason)
    {
        Transition(DataFlowState.Suspended);
    }

    public void Start()
    {
        Transition(DataFlowState.Started);
    }
}