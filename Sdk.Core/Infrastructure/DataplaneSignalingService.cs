using Sdk.Core.Domain;
using Sdk.Core.Domain.Messages;

namespace Sdk.Core.Infrastructure;

public class DataplaneSignalingService : IDataplaneSignalingService
{
    public Task<StatusResult<DataFlowResponseMessage>> Start(DataflowStartMessage message)
    {
        throw new NotImplementedException();
    }
}