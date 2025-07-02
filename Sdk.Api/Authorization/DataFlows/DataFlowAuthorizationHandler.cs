using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Sdk.Core.Domain.Interfaces;

namespace Sdk.Api.Authorization.DataFlows;

public class DataFlowAuthorizationHandler(IDataPlaneStore store)
    : AuthorizationHandler<DataFlowRequirement, ResourceTuple>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        DataFlowRequirement requirement, ResourceTuple resource)
    {
        var (participantContextId, dataFlowId) = resource;

        // Verify that the participant context ID (from the request) matches the user ID in the claims
        if (participantContextId != context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value)
        {
            context.Fail();
            return;
        }

        if (dataFlowId == null)
        {
            context.Succeed(requirement);
            return;
        }

        var dataFlow = await store.FindByIdAsync(dataFlowId);
        //if null, then its not a permission problem, but a data flow not found problem
        if (dataFlow == null || dataFlow.ParticipantId == participantContextId)
        {
            context.Succeed(requirement);
        }
    }
}