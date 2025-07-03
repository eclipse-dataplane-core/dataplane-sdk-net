namespace Sdk.Core.Domain.Model;

/// <summary>
///     Represents a data address, i.e. a physical location.
/// </summary>
public class DataAddress
{
    public DataAddress(string type)
    {
        Properties["@id"] = Guid.NewGuid().ToString();
        Properties[IConstants.EdcNamespace + "type"] = type;
    }

    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

    public string? Type => Properties[IConstants.EdcNamespace + "type"] as string;
    public string Id => Properties["@id"] as string ?? throw new InvalidOperationException();
}