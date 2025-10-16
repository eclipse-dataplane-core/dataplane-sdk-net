using HttpDataplane.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpDataplane.Controllers;

/// <summary>
///     This controller acts as the consumer-facing API for data transfers.
/// </summary>
[Route("api/v1/public")]
[ApiController]
public class PublicApiController(IDataService dataService, IHttpProxyService proxyService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<string>> Get([FromHeader(Name = "x-api-key")] string apiKey,
        [FromRoute] string id)
    {
        var flow = await dataService.GetFlow(id);
        if (flow is { Source: not null } && await dataService.IsPermitted(apiKey, flow))
        {
            if (!flow.Source.Properties.TryGetValue("baseUrl", out var url))
            {
                return BadRequest("Source URL is not set");
            }

            return await proxyService.GetData(url.ToString()!);
        }


        return Forbid();
    }
}