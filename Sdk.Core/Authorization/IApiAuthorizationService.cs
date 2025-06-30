using System.Security.Claims;

namespace Sdk.Core.Authorization;

public interface IApiAuthorizationService 
{
    public Task<bool> AuthorizeAsync(string participantContext, ClaimsPrincipal claimsPrincipal);
}