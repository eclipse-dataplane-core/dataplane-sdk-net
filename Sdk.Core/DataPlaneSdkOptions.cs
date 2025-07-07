using Sdk.Core.Infrastructure;

namespace Sdk.Core;

public class DataPlaneSdkOptions
{
    public required string RuntimeId { get; set; }
    public required string ParticipantId { get; set; }
    public Uri? PublicUrl { get; set; }
    public ICollection<string> AllowedSourceTypes { get; set; } = new List<string>();
    public ICollection<string> AllowedTransferTypes { get; set; } = new List<string>();
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();
    public ControlApiOptions? ControlApi { get; init; }
}