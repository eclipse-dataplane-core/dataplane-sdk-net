using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public IActionResult Get([FromRoute] string dataFlowId, [FromRoute] string participantContextId)
    {
        _logger.LogInformation("Participant Context ID: {ParticipantContextId}", participantContextId);
        return Ok(
            new
            {
                Message = "Data flow retrieved successfully",
                DataFlowId = dataFlowId,
                ParticipantContextId = participantContextId
            });
    }
}