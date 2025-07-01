namespace Sdk.Core.Domain.Interfaces;

public abstract class Identifiable(string id)
{
    public string Id { get; } = id;
}