using System.Buffers.Binary;

namespace codecrafters_kafka.src;

public class ErrorResponse : BaseResponse
{
    public readonly short ErrorCode;
    public ErrorResponse(int correlationID, short errorCode) : base(correlationID)
    {
        ErrorCode = errorCode;
    }

    public override byte[] ToBytes()
    {
        MessageSize = 10;
        var response = new byte[MessageSize];

        BinaryPrimitives.WriteInt32BigEndian(response.AsSpan()[..4], MessageSize);
        BinaryPrimitives.WriteInt32BigEndian(response.AsSpan()[4..8], CorrelationID);
        BinaryPrimitives.WriteInt16BigEndian(response.AsSpan()[8..10], ErrorCode);

        return response;
    }
}