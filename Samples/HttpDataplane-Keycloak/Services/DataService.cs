using DataPlane.Sdk.Core.Domain.Model;

namespace HttpDataplane.Services;

public class DataService : IDataService
{
    private readonly IDictionary<string, DataFlow> _permissions = new Dictionary<string, DataFlow>();

    public Task<bool> IsPermitted(string apiKey, DataFlow dataFlow)
    {
        return Task.FromResult(dataFlow.Destination.Properties["token"] as string == apiKey);
    }

    public Task<DataFlow?> GetFlow(string id)
    {
        _permissions.TryGetValue(id, out var flow);
        return Task.FromResult(flow);
    }

    public DataFlow SetPublicEndpoint(DataFlow dataFlow)
    {
        var id = dataFlow.Id; //todo: should this be the DataAddress ID or even randomly generated?
        var apiToken = Guid.NewGuid().ToString();
        _permissions[id] = dataFlow;

        dataFlow.State = DataFlowState.Started;
        dataFlow.Destination = new DataAddress("HttpData")
        {
            Properties =
            {
                ["url"] = $"http://localhost:8080/api/v1/public/{id}",
                ["token"] = apiToken
            }
        };
        return dataFlow;
    }
}