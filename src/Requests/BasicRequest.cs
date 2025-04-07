using System.Buffers.Binary;

namespace src.Requests;

public class BasicRequest
{
    public readonly int MessageSize;
    public readonly short APIKey;
    public readonly short APIVersion;
    public readonly int CorrelationID;

    private BasicRequest(int messageSize, short apiKey, short apiVersion, int correlationID)
    {
        MessageSize = messageSize;
        APIKey = apiKey;
        APIVersion = apiVersion;
        CorrelationID = correlationID;
    }

    public static BasicRequest Parse(byte[] headerBytes)
    {
        var messageSize = BinaryPrimitives.ReadInt32BigEndian(headerBytes.AsSpan()[0..4]);
        var requestAPIKey = BinaryPrimitives.ReadInt16BigEndian(headerBytes.AsSpan()[4..6]);
        var requestAPIVersion = BinaryPrimitives.ReadInt16BigEndian(headerBytes.AsSpan()[6..8]);
        var correlationID = BinaryPrimitives.ReadInt32BigEndian(headerBytes.AsSpan()[8..12]);

        return new BasicRequest(messageSize, requestAPIKey, requestAPIVersion, correlationID);
    }
}