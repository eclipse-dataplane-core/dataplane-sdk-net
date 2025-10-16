using DataPlane.Sdk.Core.Domain.Model;
using Microsoft.Extensions.Options;
using Provider.Nats;

namespace Provider.Services;

public class DataService(IOptions<NatsOptions> options, INatsPublisherService publisherService) : IDataService
{
    public Task<bool> IsPermitted(string apiKey, DataFlow dataFlow)
    {
        return Task.FromResult(dataFlow.Destination?.Properties["token"] as string == apiKey);
    }

    /// <summary>
    /// </summary>
    /// <param name="flow"></param>
    /// <returns></returns>
    public StatusResult<DataFlow> ProcessStart(DataFlow flow)
    {
        // create a data address for the NATS endpoint
        var channel = flow.Id + "." + Constants.ForwardChannelSuffix;
        var replyChannel = flow.Id + "." + Constants.ReplyChannelSuffix;


        var dataAddress = new NatsDataAddress
        {
            NatsEndpoint = options.Value.NatsEndpoint,
            Channel = channel,
            ReplyChannel = replyChannel
        };

        flow.Destination = dataAddress;

        // start publishing events
        publisherService.StartAsync(channel);

        return StatusResult<DataFlow>.Success(flow);
    }

    public async Task<StatusResult> ProcessTerminate(DataFlow dataFlow)
    {
        if (dataFlow.Destination != null)
        {
            var nda = NatsDataAddress.Create(dataFlow.Destination);
            await publisherService.StopAsync(nda.Channel);
            return StatusResult.Success();
        }

        return StatusResult.Failed(new StatusFailure { Message = "DataAddress is not a valid NATS DataAddress", Reason = FailureReason.InternalError });
    }
}