using src;
using src.Requests;
using src.Responses;
using System.Net;
using System.Net.Sockets;

TcpListener server = new TcpListener(IPAddress.Any, 9092);
server.Start();

while (true)
{
    var socket = server.AcceptSocket();
    Task.Run(() => HandleSocket(socket));
}


static void HandleSocket(Socket socket)
{
    while (socket.Connected)
    {
        var headerBytes = new byte[socket.ReceiveBufferSize];
        var bytesRead = socket.Receive(headerBytes);

        // First determine what kind of request we are by parsing the basic 4 properties
        var basicRequest = BasicRequest.Parse(headerBytes);

        BaseResponse response = new ErrorResponse(basicRequest.CorrelationID, ErrorCodes.UnknownServerError);

        // If the info is enough to determine we cannot handle this request then return unsupported
        if (basicRequest.APIVersion < 0 || basicRequest.APIVersion > 4) { response = new ErrorResponse(basicRequest.CorrelationID, ErrorCodes.UnsupportedVersion); }

        switch (basicRequest.APIKey)
        {
            case APIKeys.APIVersions: { response = ProcessAPIVersionsRequest(basicRequest); break; }
            case APIKeys.DescribeTopicPartitions:
                {
                    var request = DescribeTopicPartitionsRequest.Parse(headerBytes);
                    response = ProcessDescribeTopicPartitionsRequest(request);
                    break;
                }
        }

        // Last line of defense in case unexpected errors occur, to stop a ToBytes() on a null object
        response ??= new ErrorResponse(basicRequest.CorrelationID, ErrorCodes.UnknownServerError);

        socket.Send(response.ToBytes());
    }

    socket.Shutdown(SocketShutdown.Both);
    socket.Close();
}

static APIVersionsResponse ProcessAPIVersionsRequest(BasicRequest request)
{
    return new APIVersionsResponse(request.CorrelationID,
    [
        new() { APIKey = APIKeys.APIVersions, MinSupportedAPIVersion = 0, MaxSupportedAPIVersion = 4 },
        new() { APIKey = APIKeys.DescribeTopicPartitions, MinSupportedAPIVersion = 0, MaxSupportedAPIVersion = 0 }
    ]);
}

static DescribeTopicPartitionsResponse ProcessDescribeTopicPartitionsRequest(DescribeTopicPartitionsRequest request)
{
    var topics = request.Topics.Select(t => new TopicResponse
    {
        ErrorCode = ErrorCodes.UnknownTopicOrPartition,
        ID = Guid.Empty,
        IsInternal = false,
        Name = t,
        Partitions = 0
    }).ToList();
    return new DescribeTopicPartitionsResponse(request.CorrelationID, topics);
}