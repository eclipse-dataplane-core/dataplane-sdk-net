using Sdk.Core.Domain.Interfaces;

namespace Sdk.Core.Domain;

public class DataFlow(string id) : Identifiable(id)
{
    public readonly string Id = id;
    public required DataAddress Source { get; set; }
    public required DataAddress Destination { get; set; }
    public Uri? CallbackAddress { get; set; }
    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();

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
    public int State { get; set; } = -1;
    public required string AgreementId { get; set; }
    public int StateCount { get; private set; }
    public DateTime StateTimestamp { get; private set; } = DateTime.UtcNow;
    public string? ErrorDetail { get; } = null;
    public bool IsPending { get; } = false;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    public void Deprovision()
    {
        Transition((int)DataFlowState.Deprovisioning);
    }

    private void Transition(int targetState)
    {
        StateCount = State == targetState ? StateCount + 1 : 1;
        State = targetState;
        StateTimestamp = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Terminate()
    {
        Transition((int)DataFlowState.Terminated);
    }
}