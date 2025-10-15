using System.Text.Json;
using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;
using Shouldly;

namespace DataPlane.Sdk.Core.Test.Domain.Messages;

/// <summary>
///     Tests for JSON serialization and deserialization of DataFlowStartMessage
/// </summary>
public class DataFlowStartMessageTest
{
    [Fact]
    public void Serialize_WithAllProperties_Success()
    {
        // Arrange
        var message = new DataFlowStartMessage
        {
            MessageId = "msg-123",
            ProcessId = "process-456",
            DatasetId = "dataset-789",
            ParticipantId = "participant-abc",
            AgreementId = "agreement-def",
            DataAddress = new DataAddress("AzureBlob")
            {
                Properties = { ["container"] = "dest-container", ["account"] = "myaccount" }
            },
            TransferType = new TransferType
            {
                DestinationType = "Stream",
                FlowType = FlowType.Push
            }
        };

        // Act
        var json = JsonSerializer.Serialize(message);

        // Assert
        json.ShouldNotBeNullOrWhiteSpace();
        json.ShouldContain("\"messageId\":\"msg-123\"");
        json.ShouldContain("\"processId\":\"process-456\"");
        json.ShouldContain("\"datasetId\":\"dataset-789\"");
        json.ShouldContain("\"participantId\":\"participant-abc\"");
        json.ShouldContain("\"agreementId\":\"agreement-def\"");
        json.ShouldContain("\"dataAddress\"");
    }

    [Fact]
    public void Deserialize_WithAllProperties_Success()
    {
        // Arrange
        var json = """
                   {
                       "messageID": "msg-123",
                       "processID": "process-456",
                       "datasetID": "dataset-789",
                       "participantID": "participant-abc",
                       "agreementID": "agreement-def",
                       "dataAddress": {
                           "@type": "AzureBlob",
                           "properties": {
                               "container": "dest-container",
                               "account": "myaccount"
                           }
                       },
                       "transferType": {
                            "destinationType": "Stream",
                            "flowType": "Push"
                       }
                   }
                   """;

        // Act
        var message = JsonSerializer.Deserialize<DataFlowStartMessage>(json, TestJsonDeserializerConfig.DefaultOptions);

        // Assert
        message.ShouldNotBeNull();
        message.MessageId.ShouldBe("msg-123");
        message.ProcessId.ShouldBe("process-456");
        message.DatasetId.ShouldBe("dataset-789");
        message.ParticipantId.ShouldBe("participant-abc");
        message.AgreementId.ShouldBe("agreement-def");
        message.DataAddress.ShouldNotBeNull();
        message.DataAddress.Type.ShouldBe("AzureBlob");
        message.DataAddress.Properties["container"].ShouldBeEquivalentTo("dest-container");
        message.DataAddress.Properties["account"].ShouldBeEquivalentTo("myaccount");
        message.TransferType.FlowType.ShouldBe(FlowType.Push);
    }

    [Fact]
    public void Deserialize_WithMinimalProperties_Success()
    {
        // Arrange
        var json = """
                   {
                       "processID": "process-456",
                       "datasetID": "dataset-789",
                       "participantID": "participant-abc",
                       "agreementID": "agreement-def",
                       "dataAddress": {
                           "@type": "HttpData"
                       },
                       "transferType": {
                           "destinationType": "test-dest-type",
                           "flowType": "Pull" 
                       }
                   }
                   """;

        // Act
        var message = JsonSerializer.Deserialize<DataFlowStartMessage>(json, TestJsonDeserializerConfig.DefaultOptions);

        // Assert
        message.ShouldNotBeNull();
        message.ProcessId.ShouldBe("process-456");
        message.DatasetId.ShouldBe("dataset-789");
        message.ParticipantId.ShouldBe("participant-abc");
        message.AgreementId.ShouldBe("agreement-def");
        message.DataAddress.ShouldNotBeNull();
        message.DataAddress.Type.ShouldBe("HttpData");
        message.TransferType.FlowType.ShouldBe(FlowType.Pull);
    }

    [Fact]
    public void SerializeDeserialize_RoundTrip_Success()
    {
        // Arrange
        var original = new DataFlowStartMessage
        {
            MessageId = "msg-roundtrip",
            ProcessId = "process-roundtrip",
            DatasetId = "dataset-roundtrip",
            ParticipantId = "participant-roundtrip",
            AgreementId = "agreement-roundtrip",
            DataAddress = new DataAddress("Database")
            {
                Properties = { ["connectionString"] = "Server=localhost", ["table"] = "dest_table" }
            },
            TransferType = new TransferType
            {
                DestinationType = "Stream",
                FlowType = FlowType.Push
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<DataFlowStartMessage>(json, TestJsonDeserializerConfig.DefaultOptions);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.MessageId.ShouldBe(original.MessageId);
        deserialized.ProcessId.ShouldBe(original.ProcessId);
        deserialized.DatasetId.ShouldBe(original.DatasetId);
        deserialized.ParticipantId.ShouldBe(original.ParticipantId);
        deserialized.AgreementId.ShouldBe(original.AgreementId);
        deserialized.DataAddress?.Type.ShouldBe(original.DataAddress.Type);
        deserialized.DataAddress?.Properties["connectionString"].ShouldBeEquivalentTo("Server=localhost");
        deserialized.DataAddress?.Properties["table"].ShouldBeEquivalentTo("dest_table");
        deserialized.TransferType.ShouldBeEquivalentTo(original.TransferType);
    }

    [Fact]
    public void Deserialize_WithEmptyDataAddressProperties_Success()
    {
        // Arrange
        var json = """
                   {
                       "processID": "process-123",
                       "datasetID": "dataset-123",
                       "participantID": "participant-123",
                       "agreementID": "agreement-123",
                       "dataAddress": {
                           "type": "Custom",
                           "properties": {}
                       },
                       "transferType": {
                            "destinationType": "test-dest-type",
                            "flowType": "Pull"
                       }
                   }
                   """;

        // Act
        var message = JsonSerializer.Deserialize<DataFlowStartMessage>(json, TestJsonDeserializerConfig.DefaultOptions);

        // Assert
        message.ShouldNotBeNull();
        message.DataAddress.ShouldNotBeNull();
        message.DataAddress.Properties.ShouldBeEmpty();
    }

    [Fact]
    public void Serialize_WithCamelCasePropertyNames_Success()
    {
        // Arrange
        var message = new DataFlowStartMessage
        {
            ProcessId = "process-123",
            DatasetId = "dataset-123",
            ParticipantId = "participant-123",
            AgreementId = "agreement-123",
            DataAddress = new DataAddress("TestType"),
            TransferType = new TransferType
            {
                DestinationType = "test-type",
                FlowType = FlowType.Push
            }
        };

        // Act
        var json = JsonSerializer.Serialize(message);

        // Assert - should use camelCase based on JsonPropertyName attributes
        json.ShouldContain("\"processId\"");
        json.ShouldContain("\"datasetId\"");
        json.ShouldContain("\"participantId\"");
        json.ShouldContain("\"agreementId\"");
        json.ShouldContain("\"dataAddress\"");
        json.ShouldContain("\"transferType\"");
    }
}