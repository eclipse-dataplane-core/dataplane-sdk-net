using DataPlane.Sdk.Api.Authorization;
using DataPlane.Sdk.Core.Domain.Interfaces;
using DataPlane.Sdk.Core.Domain.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataPlane.Sdk.Api.Controllers;

[ApiController]
[Route("/api/v1")]
public class DataPlaneSignalingApiController(
    IDataPlaneSignalingService signalingService,
    IAuthorizationService authorizationService)
    : ControllerBase
{
    [Authorize]
    [HttpGet("{participantContextId}/dataflows/{dataFlowId}/state")]
    public async Task<IActionResult> GetTransferState([FromRoute] string dataFlowId, [FromRoute] string participantContextId)
    {
        if (!(await authorizationService.AuthorizeAsync(User, new ResourceTuple(participantContextId, dataFlowId), "DataFlowAccess")).Succeeded)
        {
            return Forbid();
        }

        var state = await signalingService.GetTransferStateAsync(dataFlowId);

        return state.IsSucceeded ? Ok(state.Content) : StatusCode((int)state.Failure!.Reason, state);
    }

    [Authorize]
    [HttpPost("{participantContextId}/dataflows/")]
    public async Task<IActionResult> StartDataFlow([FromRoute] string participantContextId, DataFlowStartMessage startMessage)
    {
        if (!(await authorizationService.AuthorizeAsync(User, new ResourceTuple(participantContextId, null), "DataFlowAccess")).Succeeded)
        {
            return Forbid();
        }

        var valid = await signalingService.ValidateStartMessageAsync(startMessage);
        if (!valid.IsSucceeded)
        {
            return BadRequest(valid.Failure?.Message);
        }

        var result = await signalingService.StartAsync(startMessage);
        if (result.IsFailed)
        {
            return BadRequest(result.Failure?.Message);
        }

        return Ok(result.Content);
    }

    [Authorize]
    [HttpPost("{participantContextId}/dataflows/{dataFlowId}/suspend")]
    public async Task<IActionResult> SuspendDataFlow([FromRoute] string dataFlowId, [FromRoute] string participantContextId,
        DataFlowSuspendMessage suspendMessage)
    {
        if (!(await authorizationService.AuthorizeAsync(User, new ResourceTuple(participantContextId, dataFlowId), "DataFlowAccess")).Succeeded)
        {
            return Forbid();
        }

        var result = await signalingService.SuspendAsync(dataFlowId, suspendMessage.Reason);
        if (result.IsFailed)
        {
            return BadRequest(result.Failure?.Message);
        }

        return Ok(result.Content);
    }

    [Authorize]
    [HttpPost("{participantContextId}/dataflows/{dataFlowId}/terminate")]
    public async Task<IActionResult> TerminateDataFlow([FromRoute] string dataFlowId, [FromRoute] string participantContextId,
        DataFlowTerminationMessage terminateMessage)
    {
        if (!(await authorizationService.AuthorizeAsync(User, new ResourceTuple(participantContextId, dataFlowId), "DataFlowAccess")).Succeeded)
        {
            return Forbid();
        }

        var result = await signalingService.TerminateAsync(dataFlowId, terminateMessage.Reason);
        if (result.IsFailed)
        {
            return BadRequest(result.Failure?.Message);
        }

        return Ok(result.Content);
    }

    [HttpGet("check")]
    public IActionResult CheckHealth()
    {
        return Ok();
    }

    [Authorize]
    [HttpGet("foo/{fooId}")]
    public async Task<IActionResult> GetFoo([FromRoute] string fooId, [FromRoute] string participantContextId)
    {
        var authorizationResult = await authorizationService.AuthorizeAsync(User, new ResourceTuple(participantContextId, fooId), "FooAccess");

        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }


        return Ok();
    }
}