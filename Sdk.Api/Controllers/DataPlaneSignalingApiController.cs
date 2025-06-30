using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sdk.Core.Domain.Interfaces;

namespace Sdk.Api.Controllers;

[ApiController]
[Route("/api/v1/{participantContextId}/dataflows")]
public class DataPlaneSignalingApiController : ControllerBase
{
    private readonly ILogger<DataPlaneSignalingApiController> _logger;
    private readonly IDataPlaneSignalingService _dataPlaneSignalingApiService;

    public DataPlaneSignalingApiController(ILogger<DataPlaneSignalingApiController> logger,
        IDataPlaneSignalingService signalingService)
    {
        _dataPlaneSignalingApiService = signalingService;
        _logger = logger;
    }

    [Authorize]
    [HttpGet(template: "{dataFlowId}")]
    public async Task<IActionResult> Get([FromRoute] string dataFlowId, [FromRoute] string participantContextId)
    {
        var state = await _dataPlaneSignalingApiService.GetTransferStateAsync(dataFlowId);

        if (state.IsSucceeded)
        {
            return Ok(state.Content);
        }

        return StatusCode(state.Failure!.Code, state);
    }
}