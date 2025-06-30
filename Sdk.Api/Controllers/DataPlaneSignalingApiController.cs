using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sdk.Core.Authorization;
using Sdk.Core.Domain.Interfaces;

namespace Sdk.Api.Controllers;

[ApiController]
[Route("/api/v1/{participantContextId}/dataflows")]
public class DataPlaneSignalingApiController(
    IDataPlaneSignalingService signalingService,
    IApiAuthorizationService authorizationService)
    : ControllerBase
{
    [Authorize]
    [HttpGet(template: "{dataFlowId}")]
    public async Task<IActionResult> Get([FromRoute] string dataFlowId, [FromRoute] string participantContextId)
    {
        
        var isAuthorized = await authorizationService.AuthorizeAsync(participantContextId, User);
       
        var state = await signalingService.GetTransferStateAsync(dataFlowId);

        if (state.IsSucceeded)
        {
            return Ok(state.Content);
        }

        return StatusCode(state.Failure!.Code, state);
    }
}