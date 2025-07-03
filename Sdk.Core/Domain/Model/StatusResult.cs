namespace Sdk.Core.Domain.Model;

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

    public static StatusResult<TContent> NotFound()
    {
        return Failed(new StatusFailure
        {
            Message = "Not Found",
            Reason = FailureReason.NotFound
        });
    }

    public static StatusResult<TContent> Conflict(string message)
    {
        return Failed(new StatusFailure
        {
            Message = message,
            Reason = FailureReason.Conflict
        });
    }
}

public class StatusFailure
{
    public required string Message { get; set; }
    public required FailureReason Reason { get; set; }
}

public enum FailureReason
{
    NotFound = 404,
    Conflict = 409,
    InternalError = 500,
    ServiceUnavailable = 503,
    Unauthorized = 401,
    Forbidden = 403,
    BadRequest = 400
}