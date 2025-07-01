using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sdk.Api.Authorization;
using Sdk.Core.Domain.Interfaces;

namespace Sdk.Api.Controllers;

[ApiController]
[Route("/api/v1/{participantContextId}")]
public class DataPlaneSignalingApiController(
    IDataPlaneSignalingService signalingService,
    IAuthorizationService authorizationService)
    : ControllerBase
{
    [Authorize]
    [HttpGet("dataflows/{dataFlowId}")]
    public async Task<IActionResult> Get([FromRoute] string dataFlowId, [FromRoute] string participantContextId)
    {
        var authorizationResult = await authorizationService.AuthorizeAsync(User,
            new ResourceTuple(participantContextId, dataFlowId), "DataFlowAccess");

        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        var state = await signalingService.GetTransferStateAsync(dataFlowId);

        if (state.IsSucceeded)
        {
            return Ok(state.Content);
        }

        return StatusCode(state.Failure!.Code, state);
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