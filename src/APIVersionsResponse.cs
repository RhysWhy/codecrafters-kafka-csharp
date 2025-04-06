using System.Buffers.Binary;

namespace codecrafters_kafka.src;

public class APIVersionsResponse : BaseResponse
{
    public readonly IReadOnlyList<APIVersion> Versions;

    public APIVersionsResponse(int correlationID, IReadOnlyList<APIVersion> versions) : base(correlationID)
    {
        Versions = versions;
    }

    public override byte[] ToBytes()
    {
        var totalVersions = Versions.Count;

        // 11 bytes for message size, correlation id, error code, number of versions, plus 12 bytes per version
        MessageSize = 11 + (12 * totalVersions);
        var response = new byte[MessageSize];

        BinaryPrimitives.WriteInt32BigEndian(response.AsSpan()[..4], MessageSize);
        BinaryPrimitives.WriteInt32BigEndian(response.AsSpan()[4..8], CorrelationID);
        BinaryPrimitives.WriteInt16BigEndian(response.AsSpan()[8..10], ErrorCodes.None);
        response.AsSpan()[10] = Convert.ToByte(totalVersions + 1);

        for (var i = 0; i < totalVersions; i++)
        {
            var version = Versions[i];
            var startIndex = 11 + (i * 12);
            BinaryPrimitives.WriteInt16BigEndian(response.AsSpan()[startIndex..(startIndex += 2)], version.APIKey);
            BinaryPrimitives.WriteInt16BigEndian(response.AsSpan()[startIndex..(startIndex += 2)], version.MinSupportedAPIVersion);
            BinaryPrimitives.WriteInt16BigEndian(response.AsSpan()[startIndex..(startIndex += 2)], version.MaxSupportedAPIVersion);
            response.AsSpan()[startIndex++] = 0; // Tag buffer
            BinaryPrimitives.WriteInt32BigEndian(response.AsSpan()[startIndex..(startIndex += 4)], 0); // Throttle time in ms
            response.AsSpan()[startIndex++] = 0; // Tag buffer
        }

        return response;
    }
}

public class APIVersion
{
    public short APIKey { get; set; }
    public short MinSupportedAPIVersion { get; set; }
    public short MaxSupportedAPIVersion { get; set; }
}