using DataPlane.Sdk.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DataPlane.Sdk.Example.Web.Controllers;

[ApiController]
[Route("/notify")]
public class DemoNotifyController(IControlPlaneSignalingClient signalingClient) : ControllerBase
{
    [HttpPost("started")]
    public async Task<IActionResult> Start([FromQuery(Name = "id")] string processId)
    {
        var res = await signalingClient.NotifyStarted(processId);
        return res.IsSucceeded ? Ok() : StatusCode((int)res.Failure!.Reason, res);
    }
}