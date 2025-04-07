namespace src.Responses;

public abstract class BaseResponse
{
    public int MessageSize { get; protected set; }
    public int CorrelationID { get; protected set; }

    protected BaseResponse(int correlationID) { CorrelationID = correlationID; }

    public abstract byte[] ToBytes();
}