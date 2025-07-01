using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Sdk.Core.Authorization.Foo;

public class FooAuthorizationHandler : AuthorizationHandler<FooRequirement, ResourceTuple>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FooRequirement requirement,
        ResourceTuple resource)
    {
        var (participantContextId, fooId) = resource;

        // Verify that the participant context ID (from the request) matches the user ID in the claims
        if (participantContextId != context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // todo: this makes no sense, but is just a placeholder
        context.Succeed(requirement);

        return Task.CompletedTask;
    }
}