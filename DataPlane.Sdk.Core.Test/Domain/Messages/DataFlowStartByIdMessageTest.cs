using System.Text.Json;
using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;
using Shouldly;

namespace DataPlane.Sdk.Core.Test.Domain.Messages;

/// <summary>
///     Tests for JSON serialization and deserialization of DataFlowStartedNotificationMessage
/// </summary>
public class DataFlowStartByIdMessageTest
{
    [Fact]
    public void SerDes_WithSourceDataAddress_Success()
    {
        // Arrange
        var message = new DataFlowStartedNotificationMessage
        {
            DataAddress = new DataAddress("S3")
            {
                Properties = { ["bucketName"] = "source-bucket", ["region"] = "us-east-1" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(message);

        // Assert
        json.ShouldNotBeNullOrWhiteSpace();
        json.ShouldContain("\"dataAddress\"");
        json.ShouldContain("\"@type\":\"S3\"");
        json.ShouldContain("\"bucketName\":\"source-bucket\"");
        json.ShouldContain("\"region\":\"us-east-1\"");

        // Act
        var deserialized = JsonSerializer.Deserialize<DataFlowStartedNotificationMessage>(json, TestJsonDeserializerConfig.DefaultOptions);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.DataAddress.ShouldNotBeNull();
        deserialized.DataAddress.Type.ShouldBe("S3");
        deserialized.DataAddress.Properties["bucketName"].ShouldBeEquivalentTo("source-bucket");
        deserialized.DataAddress.Properties["region"].ShouldBeEquivalentTo("us-east-1");
        deserialized.ShouldBeEquivalentTo(message);
    }


    [Fact]
    public void SerDes_WithEmptySourceDataAddressProperties_Success()
    {
        // Arrange
        var json = """
                   {
                       "dataAddress": {
                           "@type": "HttpData"
                       }
                   }
                   """;

        // Act
        var message = JsonSerializer.Deserialize<DataFlowStartedNotificationMessage>(json, TestJsonDeserializerConfig.DefaultOptions);

        // Assert
        message.ShouldNotBeNull();
        message.DataAddress.ShouldNotBeNull();
        message.DataAddress.Type.ShouldBe("HttpData");
        message.DataAddress.Properties.ShouldBeEmpty();
    }

    [Fact]
    public void SerializeDeserialize_NoSourceDataAddress_Failure()
    {
        // Arrange
        var json = """
                   {
                   }
                   """;

        // Act
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<DataFlowStartedNotificationMessage>(json, TestJsonDeserializerConfig.DefaultOptions));
    }
}