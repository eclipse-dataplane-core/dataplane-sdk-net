using DataPlane.Sdk.Api.Authorization;
using DataPlane.Sdk.Core.Domain.Interfaces;
using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataPlane.Sdk.Api.Controllers;

[ApiController]
[Route("/api/v1/{participantContextId}/dataflows")]
public class DataPlaneSignalingApiControllerV2(
    IDataPlaneSignalingService signalingService,
    IAuthorizationService authorizationService)
    : ControllerBase
{
    [Authorize]
    [HttpPost("prepare")]
    public async Task<IActionResult> Prepare([FromRoute] string participantContextId, DataFlowPrepareMessage prepareMessage)
    {
        if (!(await authorizationService.AuthorizeAsync(User, new ResourceTuple(participantContextId, null), "DataFlowAccess")).Succeeded)
        {
            return Forbid();
        }

        var statusResult = await signalingService.PrepareAsync(prepareMessage);

        if (statusResult.IsSucceeded)
        {
            var dataFlow = statusResult.Content!;
            return dataFlow.State switch
            {
                DataFlowState.Preparing => Accepted(new Uri($"/api/v1/{participantContextId}/dataflows/{dataFlow.Id}", UriKind.Relative),
                    new DataFlowResponseMessage { DataAddress = dataFlow.Destination }),
                DataFlowState.Prepared => Ok(),
                _ => BadRequest($"DataFlow state {dataFlow.State} is not expected")
            };
        }

        return StatusCode((int)statusResult.Failure!.Reason, statusResult);
    }

    [Authorize]
    [HttpPost("{dataFlowId}/start")]
    public async Task<IActionResult> Start([FromRoute] string participantContextId, [FromRoute] string dataFlowId, DataFlowStartMessage startMessage)
    {
        if (!(await authorizationService.AuthorizeAsync(User, new ResourceTuple(participantContextId, dataFlowId), "DataFlowAccess")).Succeeded)
        {
            return Forbid();
        }


        var statusResult = await signalingService.StartAsync(startMessage);
        if (statusResult.IsSucceeded)
        {
            var dataFlow = statusResult.Content!;
            return dataFlow.State switch
            {
                DataFlowState.Starting => Accepted(new Uri($"/api/v1/{participantContextId}/dataflows/{dataFlow.Id}", UriKind.Relative)),
                DataFlowState.Started => Ok(),
                _ => BadRequest($"DataFlow state {dataFlow.State} is not expected")
            };
        }

        return StatusCode((int)statusResult.Failure!.Reason, statusResult);
    }

    [Authorize]
    [HttpPost("{dataFlowId}/suspend")]
    public async Task<IActionResult> Suspend([FromRoute] string participantContextId, [FromRoute] string dataFlowId, DataFlowSuspendMessage suspendMessage)
    {
        if (!(await authorizationService.AuthorizeAsync(User, new ResourceTuple(participantContextId, dataFlowId), "DataFlowAccess")).Succeeded)
        {
            return Forbid();
        }

        var statusResult = await signalingService.SuspendAsync(dataFlowId, suspendMessage.Reason);
        return statusResult.IsSucceeded ? Ok() : StatusCode((int)statusResult.Failure!.Reason, statusResult);
    }

    [Authorize]
    [HttpPost("{dataFlowId}/terminate")]
    public async Task<IActionResult> Terminate([FromRoute] string participantContextId, [FromRoute] string dataFlowId,
        DataFlowTerminationMessage terminateMessage)
    {
        if (!(await authorizationService.AuthorizeAsync(User, new ResourceTuple(participantContextId, dataFlowId), "DataFlowAccess")).Succeeded)
        {
            return Forbid();
        }

        var statusResult = await signalingService.TerminateAsync(dataFlowId, terminateMessage.Reason);
        return statusResult.IsSucceeded ? Ok() : StatusCode((int)statusResult.Failure!.Reason, statusResult);
    }

    [Authorize]
    [HttpGet("{dataFlowId}")]
    public async Task<IActionResult> GetStatus([FromRoute] string dataFlowId, [FromRoute] string participantContextId)
    {
        if (!(await authorizationService.AuthorizeAsync(User, new ResourceTuple(participantContextId, dataFlowId), "DataFlowAccess")).Succeeded)
        {
            return Forbid();
        }

        var state = await signalingService.GetTransferStateAsync(dataFlowId);

        return state.IsSucceeded
            ? Ok(new DataFlowStatusResponseMessage
            {
                Id = dataFlowId,
                State = state.Content
            })
            : StatusCode((int)state.Failure!.Reason, state);
    }
}