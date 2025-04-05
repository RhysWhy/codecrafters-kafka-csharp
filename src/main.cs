using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;

TcpListener server = new TcpListener(IPAddress.Any, 9092);
server.Start();

while (true)
{
    var socket = server.AcceptSocket();
    Task.Run(() => HandleSocket(socket));
}


static void HandleSocket(Socket socket)
{
    var correlationIDBuffer = new byte[4];
    var bytesRead = socket.Receive(correlationIDBuffer);

    var messageSize = 0;
    var correlationID = 7;

    var response = new byte[8];

    // First four bytes are for message size
    var sizeBytes = BitConverter.GetBytes(messageSize).Reverse().ToArray();
    Array.Copy(sizeBytes, 0, response, 0, 4);

    // Second four bytes are for correlation id
    var correlationIDBytes = BitConverter.GetBytes(correlationID).Reverse().ToArray();
    Array.Copy(correlationIDBytes, 0, response, 4, 4);

    socket.Send(response);
    socket.Shutdown(SocketShutdown.Both);
    socket.Close();
}