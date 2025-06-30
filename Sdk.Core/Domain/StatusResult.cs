using Sdk.Core.Domain.Messages;

namespace Sdk.Core.Domain;

public class StatusResult<TContent>(TContent? content, StatusFailure? failure) 
    : AbstractResult<TContent, StatusFailure>(content, failure)
{
    public static StatusResult<TContent> Success(TContent? content)
    {
        return new StatusResult<TContent>(content, null);
    }
    
    public static StatusResult<TContent> Failed(StatusFailure failure)
    {
        return new StatusResult<TContent>(default, failure);
    }
}

public class StatusFailure
{
    public required string Message { get; set; }
}