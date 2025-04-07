using System.Buffers.Binary;
using System.Text;

namespace src.Responses;

public class DescribeTopicPartitionsResponse : BaseResponse
{
    public readonly IReadOnlyList<TopicResponse> Topics;

    public DescribeTopicPartitionsResponse(int correlationID, IReadOnlyList<TopicResponse> topics) : base(correlationID)
    {
        Topics = topics;
    }

    public override byte[] ToBytes()
    {
        var totalTopics = Topics.Count;

        // 14 bytes for message size, correlation id, throttle, number of topics, plus 26 bytes per topic, plus the sum of the length of name per topic
        var totalSize = 14 + (26 * totalTopics) + Topics.Sum(t => t.NameAsBytes.Length);
        // Message size should not contain the 4 bytes needed for itself
        MessageSize = totalSize - 4;
        var response = new byte[totalSize];

        var index = 0;

        BinaryPrimitives.WriteInt32BigEndian(response.AsSpan()[index..(index += 4)], MessageSize);
        BinaryPrimitives.WriteInt32BigEndian(response.AsSpan()[index..(index += 4)], CorrelationID);
        response.AsSpan()[index++] = 0; // Tag buffer

        BinaryPrimitives.WriteInt32BigEndian(response.AsSpan()[index..(index += 4)], 0); // Throttle time in ms
        response.AsSpan()[index++] = Convert.ToByte(totalTopics + 1); // Array length + 1

        for (var i = 0; i < totalTopics; i++)
        {
            var topic = Topics[i];
            BinaryPrimitives.WriteInt16BigEndian(response.AsSpan()[index..(index += 2)], ErrorCodes.UnknownTopicOrPartition); // (2 Bytes)

            response.AsSpan()[index++] = Convert.ToByte(topic.NameAsBytes.Length + 1); // Length of name (1 byte)
            Array.Copy(topic.NameAsBytes, 0, response, index, topic.NameAsBytes.Length); // Copy the name in (n Bytes) 
            index += topic.NameAsBytes.Length;

            var idBytes = topic.ID.ToByteArray(true);
            Array.Copy(idBytes, 0, response, index, idBytes.Length); // Copy the id in (16 Bytes)
            index += idBytes.Length;

            response.AsSpan()[index++] = Convert.ToByte(topic.IsInternal); // Is Internal (1 byte)
            response.AsSpan()[index++] = Convert.ToByte(topic.Partitions + 1); // Number of Partitions (1 byte)
            
            response.AsSpan()[index++] = 0; // 4 Bytes of 0 for Authorized Operations
            response.AsSpan()[index++] = 0; // 4 Bytes of 0 for Authorized Operations
            response.AsSpan()[index++] = 0; // 4 Bytes of 0 for Authorized Operations
            response.AsSpan()[index++] = 0; // 4 Bytes of 0 for Authorized Operations

            response.AsSpan()[index++] = 0; // Tag buffer
        }

        response.AsSpan()[index++] = Convert.ToByte(null); // Next Cursor
        response.AsSpan()[index++] = 0; // Tag buffer

        return response;
    }
}

public class TopicResponse
{
    public short ErrorCode { get; set; }
    public string Name { get; set; }
    public byte[] NameAsBytes { get => Encoding.UTF8.GetBytes(Name); }
    public Guid ID { get; set; }
    public bool IsInternal { get; set; }
    public int Partitions { get; set; }
}