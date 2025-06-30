using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Infrastructure;
using Void = Sdk.Core.Domain.Void;

namespace Sdk.Core;

public class DataPlaneSdk
{
    
    public IDataPlaneStore Store { get; set; } = new DataPlaneStore();

    public Func<string, ClaimsPrincipal, Task<bool>> AuthorizationHandler { get; set; } = (s, principal) =>
    {
        var subjectClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        var isValid = subjectClaim != null && subjectClaim.Value.Equals(s);
        
        return Task.FromResult(isValid);
    };

    public event Func<JsonWebToken, Task> OnAuthentication = _ => Task.CompletedTask;

    public event Func<DataFlow, StatusResult<DataFlowResponseMessage>>? OnStart;
    public event Func<DataFlow, StatusResult<DataFlowResponseMessage>>? OnProvision;
    public event Func<DataFlow, StatusResult<Void>>? OnTerminate;
    public event Func<DataFlow, StatusResult<Void>>? OnSuspend;
    public event Func<DataFlow, StatusResult<Void>>? OnRecover;

    
    public static SdkBuilder Builder() => new();

    public class SdkBuilder
    {
        private readonly DataPlaneSdk _dataPlaneSdk = new()
        {
            OnStart = _ => StatusResult<DataFlowResponseMessage>.Success(null)
        };

        public SdkBuilder Store(DataPlaneStore dataPlaneStore)
        {
            _dataPlaneSdk.Store = dataPlaneStore;
            return this;
        }

        public DataPlaneSdk Build()
        {
            return _dataPlaneSdk;
        }

        public SdkBuilder OnStart(Func<DataFlow, StatusResult<DataFlowResponseMessage>> processor)
        {
            _dataPlaneSdk.OnStart += processor;
            return this;
        }

        public SdkBuilder OnProvision(Func<DataFlow, StatusResult<DataFlowResponseMessage>> processor)
        {
            _dataPlaneSdk.OnProvision += processor;
            return this;
        }

        public SdkBuilder OnTerminate(Func<DataFlow, StatusResult<Void>> processor)
        {
            _dataPlaneSdk.OnTerminate += processor;
            return this;
        }

        public SdkBuilder OnSuspend(Func<DataFlow, StatusResult<Void>> processor)
        {
            _dataPlaneSdk.OnSuspend += processor;
            return this;
        }

        public SdkBuilder OnRecover(Func<DataFlow, StatusResult<Void>> processor)
        {
            _dataPlaneSdk.OnRecover += processor;
            return this;
        }
    }

    internal StatusResult<Void> InvokeTerminate(DataFlow df)
    {
       return OnTerminate != null ? OnTerminate(df) : StatusResult<Void>.Success(default);
    }
}