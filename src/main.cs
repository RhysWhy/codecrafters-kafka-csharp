using codecrafters_kafka.src;
using System.Buffers.Binary;
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
    var headerBytes = new byte[socket.ReceiveBufferSize];
    var bytesRead = socket.Receive(headerBytes);

    var messageSize = BinaryPrimitives.ReadInt32BigEndian(headerBytes[0..4]);
    var requestAPIKey = BinaryPrimitives.ReadInt16BigEndian(headerBytes[4..6]);
    var requestAPIVersion = BinaryPrimitives.ReadInt16BigEndian(headerBytes[6..8]);
    var correlationID = BinaryPrimitives.ReadInt32BigEndian(headerBytes[8..12]);

    var response = new byte[10];
    short errorCode = 0;

    if(requestAPIVersion < 0 || requestAPIVersion > 4) { errorCode = StatusCodes.UnsupportedVersion; }

    // First four bytes are for message size
    var sizeBytes = BitConverter.GetBytes(messageSize).Reverse().ToArray();
    Array.Copy(sizeBytes, 0, response, 0, 4);

    // Second four bytes are for correlation id
    var correlationIDBytes = BitConverter.GetBytes(correlationID).Reverse().ToArray();
    Array.Copy(correlationIDBytes, 0, response, 4, 4);

    // Last 2 bytes are for error code
    var errorCodeBytes = BitConverter.GetBytes(errorCode).Reverse().ToArray();
    Array.Copy(errorCodeBytes, 0, response, 8, 2);

    socket.Send(response);
    socket.Shutdown(SocketShutdown.Both);
    socket.Close();
}