namespace Sdk.Core.Domain;

/// <summary>
///     Represents a data address, i.e. a physical location.
/// </summary>
public class DataAddress
{
    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

    public string? Type => Properties[IConstants.EdcNamespace + "type"] as string;
}