using Microsoft.AspNetCore.Authorization;
using Sdk.Core.Domain.Interfaces;

namespace Sdk.Core.Authorization;

public class DataFlowAuthorizationHandler(IDataPlaneStore store)
    : AuthorizationHandler<DataFlowRequirement, ResourceTuple>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        DataFlowRequirement requirement, ResourceTuple resource)
    {
        var (participantContextId, dataFlowId) = resource;

        var dataFlow = await store.FindByIdAsync(dataFlowId);
        if (dataFlow != null && dataFlow.ParticipantId == participantContextId)
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail();
    }
}