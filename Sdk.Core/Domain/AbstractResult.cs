namespace Sdk.Core.Domain;

public abstract class AbstractResult<TContent, TFailure>(TContent? content, TFailure? failure)
{
    public TContent? Content { get; set; } = content;
    public TFailure? Failure { get; set; } = failure;

    public bool IsSucceeded =>  Failure == null;
    public bool IsFailed => !IsSucceeded;


}