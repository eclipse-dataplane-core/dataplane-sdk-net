using Microsoft.AspNetCore.Authorization;

namespace Sdk.Core.Authorization;

public class FooAuthorizationHandler : AuthorizationHandler<FooRequirement, ResourceTuple>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FooRequirement requirement,
        ResourceTuple resource)
    {
        var (participantContextId, fooId) = resource;

        // todo: this makes no sense, but is just a placeholder
        if (participantContextId == fooId)
            context.Succeed(requirement);
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}