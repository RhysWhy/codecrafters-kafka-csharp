using System.Buffers.Binary;
using System.Text;

namespace src.Requests;

public class DescribeTopicPartitionsRequest
{
    public readonly int MessageSize;
    public readonly short APIKey;
    public readonly short APIVersion;
    public readonly int CorrelationID;

    public readonly string ClientID;
    public readonly IReadOnlyList<string> Topics;
    public readonly int ResponsePartitionLimit;

    private DescribeTopicPartitionsRequest(int messageSize, short apiKey, short apiVersion, int correlationID, string clientID, List<string> topics, int responsePartitionLimit)
    {
        MessageSize = messageSize;
        APIKey = apiKey;
        APIVersion = apiVersion;
        CorrelationID = correlationID;

        ClientID = clientID;
        Topics = topics;
        ResponsePartitionLimit = responsePartitionLimit;
    }

    public static DescribeTopicPartitionsRequest Parse(byte[] headerBytes)
    {
        var messageSize = BinaryPrimitives.ReadInt32BigEndian(headerBytes.AsSpan()[0..4]);
        var requestAPIKey = BinaryPrimitives.ReadInt16BigEndian(headerBytes.AsSpan()[4..6]);
        var requestAPIVersion = BinaryPrimitives.ReadInt16BigEndian(headerBytes.AsSpan()[6..8]);
        var correlationID = BinaryPrimitives.ReadInt32BigEndian(headerBytes.AsSpan()[8..12]);

        var clientIDLength = BinaryPrimitives.ReadInt16BigEndian(headerBytes.AsSpan()[12..14]);
        var clientID = Encoding.UTF8.GetString(headerBytes.AsSpan()[14..(14 + clientIDLength)]);

        // offset is 14 (where we were at the start of the client id, plus client id length, plus 1 byte of tag buffer)
        var offset = 14 + clientIDLength + 1;

        // A topics array is always of length 1 greater than the number of elements (length 1 is 0 elements, length 2 is 1 element, etc)
        var topicsArrayLength = Convert.ToInt16(headerBytes.AsSpan()[offset++]) - 1;

        // Loop over each topic
        var topics = new List<string>();
        for (var i = 0; i < topicsArrayLength; i++)
        {
            // Length is 1 greater (as always)
            var topicNameLength = Convert.ToInt16(headerBytes.AsSpan()[offset++]) - 1;
            var topicName = Encoding.UTF8.GetString(headerBytes.AsSpan()[offset..(offset += topicNameLength)]);

            topics.Add(topicName);

            offset += 1; // Increment offset by 1 byte of tag buffer
        }

        var responsePartitionLimit = BinaryPrimitives.ReadInt32BigEndian(headerBytes.AsSpan()[offset..(offset += 4)]);

        return new DescribeTopicPartitionsRequest(messageSize, requestAPIKey, requestAPIVersion, correlationID, clientID, topics, responsePartitionLimit);
    }
}