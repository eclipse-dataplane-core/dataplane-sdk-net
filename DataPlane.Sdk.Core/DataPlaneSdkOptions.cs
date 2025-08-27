using DataPlane.Sdk.Core.Infrastructure;

namespace DataPlane.Sdk.Core;

public class DataPlaneSdkOptions
{
    public required string RuntimeId { get; set; }
    public ICollection<string> AllowedSourceTypes { get; set; } = new List<string>();
    public ICollection<string> AllowedTransferTypes { get; set; } = new List<string>();
    public string DataplaneId { get; set; } = Guid.NewGuid().ToString();
    public required ControlApiOptions ControlApi { get; init; }
}