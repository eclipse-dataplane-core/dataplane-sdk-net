using System.Text.Json;
using DataPlane.Sdk.Core.Domain.Messages;
using Shouldly;

namespace DataPlane.Sdk.Core.Test.Domain.Messages;

public class DataFlowTerminateMessageTest
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("test reason")]
    [InlineData("         ")]
    public void SerializeDeserialize(string? reason)
    {
        // Arrange
        var message = new DataFlowTerminateMessage
        {
            Reason = reason
        };

        // Act
        var json = JsonSerializer.Serialize(message);

        json.ShouldNotBeNullOrWhiteSpace();
        if (reason != null)
        {
            json.ShouldContain($"\"reason\":\"{reason}\"");
        }

        var deserialized = JsonSerializer.Deserialize<DataFlowTerminateMessage>(json);

        deserialized.ShouldNotBeNull();
        deserialized.ShouldBeEquivalentTo(message);
    }
}