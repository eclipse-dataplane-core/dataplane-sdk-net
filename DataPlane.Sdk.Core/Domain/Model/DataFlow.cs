namespace DataPlane.Sdk.Core.Domain.Model;

public class DataFlow(string id) : StatefulEntity<DataFlowState>(id)
{
    public DataAddress? Source { get; set; }
    public required DataAddress? Destination { get; set; }
    public Uri? CallbackAddress { get; init; }
    public IDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();

    public required TransferType TransferType { get; init; }
    public required string RuntimeId { get; init; }
    public bool IsProvisionComplete { get; init; } = true;
    public bool IsProvisionRequested { get; init; }
    public bool IsDeprovisionComplete { get; init; }
    public bool IsDeprovisionRequested { get; init; }
    public bool IsConsumer { get; set; }
    public required string ParticipantId { get; init; }
    public required string AssetId { get; init; }
    public required string AgreementId { get; init; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public List<ProvisionResource> ResourceDefinitions { get; } = [];


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


    public void AddResourceDefinitions(IList<ProvisionResource> resources)
    {
        ResourceDefinitions.AddRange(resources);
    }

    public void Starting()
    {
        Transition(DataFlowState.Starting);
    }

    public void Complete()
    {
        Transition(DataFlowState.Completed);
    }
}