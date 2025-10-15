using System.Text.Json;
using DataPlane.Sdk.Core.Domain.Messages;
using DataPlane.Sdk.Core.Domain.Model;
using Shouldly;

namespace DataPlane.Sdk.Core.Test.Domain.Messages;

public class DataFlowPrepareMessageTest
{
    [Fact]
    public void Serialize_WithAllProperties_Success()
    {
        // Arrange
        var message = new DataFlowPrepareMessage
        {
            MessageId = "msg-123",
            ProcessId = "process-456",
            DatasetId = "dataset-789",
            ParticipantId = "participant-abc",
            CounterPartyId = "counterparty-xyz",
            DataspaceContext = "dataspace-context",
            AgreementId = "agreement-def",
            CallbackAddress = new Uri("https://callback.example.com"),
            DataAddress = new DataAddress("AzureBlob")
            {
                Properties = { ["container"] = "dest-container", ["account"] = "myaccount" }
            },
            TransferType = new TransferType
            {
                DestinationType = "AzureBlob",
                FlowType = FlowType.Push
            }
        };

        // Act
        var json = JsonSerializer.Serialize(message);

        // Assert
        json.ShouldNotBeNullOrWhiteSpace();
        json.ShouldContain("\"messageID\":\"msg-123\"");
        json.ShouldContain("\"processID\":\"process-456\"");
        json.ShouldContain("\"datasetID\":\"dataset-789\"");
        json.ShouldContain("\"participantID\":\"participant-abc\"");
        json.ShouldContain("\"counterPartyID\":\"counterparty-xyz\"");
        json.ShouldContain("\"agreementID\":\"agreement-def\"");
        json.ShouldContain("\"dataAddress\"");
        json.ShouldContain("\"transferType\"");
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
                       "counterPartyID": "counterparty-xyz",
                       "dataspaceContext": "dataspace-context",
                       "agreementID": "agreement-def",
                       "callbackAddress": "https://callback.example.com",
                       "dataAddress": {
                           "@type": "AzureBlob",
                           "properties": {
                               "container": "dest-container",
                               "account": "myaccount"
                           }
                       },
                       "transferType": {
                           "destinationType": "AzureBlob",
                           "flowType": "Push"
                       }
                   }
                   """;

        // Act
        var message = JsonSerializer.Deserialize<DataFlowPrepareMessage>(json);

        // Assert
        message.ShouldNotBeNull();
        message.MessageId.ShouldBe("msg-123");
        message.ProcessId.ShouldBe("process-456");
        message.DatasetId.ShouldBe("dataset-789");
        message.ParticipantId.ShouldBe("participant-abc");
        message.CounterPartyId.ShouldBe("counterparty-xyz");
        message.DataspaceContext.ShouldBe("dataspace-context");
        message.AgreementId.ShouldBe("agreement-def");
        message.CallbackAddress.ShouldNotBeNull();
        message.CallbackAddress.ToString().ShouldBe("https://callback.example.com/");
        message.DataAddress.ShouldNotBeNull();
        message.DataAddress.Type.ShouldBe("AzureBlob");
        message.TransferType.ShouldNotBeNull();
        message.TransferType.DestinationType.ShouldBe("AzureBlob");
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
                           "type": "HttpData"
                       },
                       "transferType": {
                           "destinationType": "HttpData",
                           "flowType": "Pull"
                       }
                   }
                   """;

        // Act
        var message = JsonSerializer.Deserialize<DataFlowPrepareMessage>(json);

        // Assert
        message.ShouldNotBeNull();
        message.ProcessId.ShouldBe("process-456");
        message.DatasetId.ShouldBe("dataset-789");
        message.ParticipantId.ShouldBe("participant-abc");
        message.AgreementId.ShouldBe("agreement-def");
        message.DataAddress.ShouldNotBeNull();
        message.TransferType.ShouldNotBeNull();
        message.TransferType.FlowType.ShouldBe(FlowType.Pull);
    }

    [Fact]
    public void SerializeDeserialize_RoundTrip_Success()
    {
        // Arrange
        var original = new DataFlowPrepareMessage
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
                DestinationType = "Database",
                FlowType = FlowType.Push
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<DataFlowPrepareMessage>(json);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.MessageId.ShouldBe(original.MessageId);
        deserialized.ProcessId.ShouldBe(original.ProcessId);
        deserialized.DatasetId.ShouldBe(original.DatasetId);
        deserialized.ParticipantId.ShouldBe(original.ParticipantId);
        deserialized.AgreementId.ShouldBe(original.AgreementId);
        deserialized.DataAddress?.Type.ShouldBe(original.DataAddress.Type);
        deserialized.TransferType.DestinationType.ShouldBe(original.TransferType.DestinationType);
        deserialized.TransferType.FlowType.ShouldBe(original.TransferType.FlowType);
    }
}