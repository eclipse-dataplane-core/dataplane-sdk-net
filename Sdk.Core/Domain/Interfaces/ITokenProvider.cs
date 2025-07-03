namespace Sdk.Core.Domain.Interfaces;

public interface ITokenProvider
{
    Task<string> GetTokenAsync();
}