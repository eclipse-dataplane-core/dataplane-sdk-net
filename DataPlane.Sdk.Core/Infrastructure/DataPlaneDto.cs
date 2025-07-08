using System.Text.Json.Serialization;
using DataPlane.Sdk.Core.Domain;
using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;

namespace DataPlane.Sdk.Core.Infrastructure;

public class DataPlaneDto : JsonLdDto
{
    public DataPlaneDto(DataPlaneInstance dataplaneInstance) : base("DataPlaneInstance")
    {
        AllowedSourceTypes = dataplaneInstance.AllowedSourceTypes;
        AllowedTransferTypes = dataplaneInstance.AllowedTransferTypes;
        LastActive = dataplaneInstance.LastActive;
        Properties = dataplaneInstance.Properties;
        Url = dataplaneInstance.Url;
        Id = dataplaneInstance.Id;
    }

    [JsonPropertyName("@id")]
    public string Id { get; init; }


    [JsonPropertyName(IConstants.EdcNamespace + "allowedSourceTypes")]
    public ICollection<string> AllowedSourceTypes { get; }

    [JsonPropertyName(IConstants.EdcNamespace + "allowedTransferTypes")]
    public ICollection<string> AllowedTransferTypes { get; }

    [JsonPropertyName(IConstants.EdcNamespace + "lastActive")]
    public long LastActive { get; }

    [JsonPropertyName(IConstants.EdcNamespace + "properties")]
    public IDictionary<string, object> Properties { get; }

    [JsonPropertyName(IConstants.EdcNamespace + "url")]
    public Uri? Url { get; }
}