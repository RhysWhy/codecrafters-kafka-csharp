using System.Buffers.Binary;

namespace codecrafters_kafka.src;

public class Request
{
    public readonly int MessageSize;
    public readonly short APIKey;
    public readonly short APIVersion;
    public readonly int CorrelationID;

    private Request(int messageSize, short apiKey, short apiVersion, int correlationID)
    {
        MessageSize = messageSize;
        APIKey = apiKey;
        APIVersion = apiVersion;
        CorrelationID = correlationID;
    }

    public static Request Parse(byte[] headerBytes)
    {
        var messageSize = BinaryPrimitives.ReadInt32BigEndian(headerBytes[0..4]);
        var requestAPIKey = BinaryPrimitives.ReadInt16BigEndian(headerBytes[4..6]);
        var requestAPIVersion = BinaryPrimitives.ReadInt16BigEndian(headerBytes[6..8]);
        var correlationID = BinaryPrimitives.ReadInt32BigEndian(headerBytes[8..12]);

        return new Request(messageSize, requestAPIKey, requestAPIVersion, correlationID);
    }
}