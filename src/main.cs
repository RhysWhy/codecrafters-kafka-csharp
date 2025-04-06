using codecrafters_kafka.src;
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

        var request = Request.Parse(headerBytes);

        var response = ProcessRequest(request);

        socket.Send(response.ToBytes());
    }

    socket.Shutdown(SocketShutdown.Both);
    socket.Close();
}

static BaseResponse ProcessRequest(Request request)
{
    if (request.APIVersion < 0 || request.APIVersion > 4) { return new ErrorResponse(request.CorrelationID, ErrorCodes.UnsupportedVersion); }

    switch (request.APIKey)
    {
        case 18:
            {
                return new APIVersionsResponse(request.CorrelationID, new List<APIVersion>
                {
                    new APIVersion
                    {
                        APIKey = APIKeys.APIVersions,
                        MinSupportedAPIVersion = 0,
                        MaxSupportedAPIVersion = 4
                    },
                    new APIVersion
                    {
                        APIKey = APIKeys.DescribeTopicPartitions,
                        MinSupportedAPIVersion = 0,
                        MaxSupportedAPIVersion = 0
                    }
                });
            }
    }

    return new ErrorResponse(request.CorrelationID, ErrorCodes.UnknownServerError);
}