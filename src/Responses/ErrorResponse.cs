using System.Buffers.Binary;

namespace src.Responses;

public class ErrorResponse : BaseResponse
{
    public readonly short ErrorCode;
    public ErrorResponse(int correlationID, short errorCode) : base(correlationID)
    {
        ErrorCode = errorCode;
    }

    public override byte[] ToBytes()
    {
        var totalSize = 10;
        // Message size should not contain the 4 bytes needed for itself
        MessageSize = totalSize - 4;
        var response = new byte[totalSize];

        BinaryPrimitives.WriteInt32BigEndian(response.AsSpan()[..4], MessageSize);
        BinaryPrimitives.WriteInt32BigEndian(response.AsSpan()[4..8], CorrelationID);
        BinaryPrimitives.WriteInt16BigEndian(response.AsSpan()[8..10], ErrorCode);

        return response;
    }
}