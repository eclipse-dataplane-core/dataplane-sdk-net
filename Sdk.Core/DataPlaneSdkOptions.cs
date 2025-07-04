namespace Sdk.Core;

public class DataPlaneSdkOptions
{
    public string RuntimeId { get; set; }
    public string ParticipantId { get; set; }
    public Uri PublicUrl { get; set; }
    public ICollection<string> AllowedSourceTypes { get; set; } = new List<string>();
    public ICollection<string> AllowedTransferTypes { get; set; } = new List<string>();
    public string InstanceId { get; set; } = Guid.NewGuid().ToString();
}