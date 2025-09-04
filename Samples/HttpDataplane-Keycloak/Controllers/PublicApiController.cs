using Microsoft.AspNetCore.Mvc;

namespace SimpleWeb_Keycloak.Controllers;

/// <summary>
///     This controller acts as the consumer-facing API for data transfers.
/// </summary>
[Route("api/v1/public")]
[ApiController]
public class PublicApiController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        return "Hello World!";
    }
}