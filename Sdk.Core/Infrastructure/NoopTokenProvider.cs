using Sdk.Core.Domain.Interfaces;

namespace Sdk.Core.Infrastructure;

public class NoopTokenProvider : ITokenProvider
{
    public Task<string> GetTokenAsync()
    {
        return Task.FromResult("REPLACE WITH ACTUAL TOKEN PROVIDER");
    }
}