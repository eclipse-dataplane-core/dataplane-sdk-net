namespace DataPlane.Sdk.Core.Domain.Messages;

public class DataFlowErroredMessage
{
    public required string Reason { get; set; }
}