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
    var headerBytes = new byte[12];
    var bytesRead = socket.Receive(headerBytes);

    var messageSize = Convert.ToInt32(headerBytes[0..4]);
    var requestAPIKey = Convert.ToInt16(headerBytes[4..6]);
    var requestAPIVersion = Convert.ToInt16(headerBytes[6..8]);
    var correlationID = Convert.ToInt32(headerBytes[8..12]);

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