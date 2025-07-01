using System.Text.Json.Serialization;

namespace Sdk.Core.Domain;

public abstract class AbstractResult<TContent, TFailure>(TContent? content, TFailure? failure)
{
    public TContent? Content { get; set; } = content;
    public TFailure? Failure { get; set; } = failure;

    [JsonIgnore]
    public bool IsSucceeded => Failure == null;

    [JsonIgnore]
    public bool IsFailed => !IsSucceeded;
}