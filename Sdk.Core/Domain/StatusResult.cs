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

    public static StatusResult<TContent> NotFound()
    {
        return Failed(new StatusFailure
        {
            Message = "Not Found",
            Code = 404
        });
    }
}

public class StatusFailure
{
    public required string Message { get; set; }
    public required int Code { get; set; }
}