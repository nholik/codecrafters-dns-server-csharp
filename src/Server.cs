using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
// Resolve UDP address
IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
int port = 2053;
IPEndPoint udpEndPoint = new IPEndPoint(ipAddress, port);

// Create UDP socket
UdpClient udpClient = new UdpClient(udpEndPoint);

while (true)
{
    // Receive data
    IPEndPoint sourceEndPoint = new IPEndPoint(IPAddress.Any, 0);
    byte[] receivedData = udpClient.Receive(ref sourceEndPoint);
    string receivedString = Encoding.ASCII.GetString(receivedData);

    Console.WriteLine($"Received {receivedData.Length} bytes from {sourceEndPoint}: {receivedString}");

    var message = new DnsMessage(receivedData[0..3]);
    message.QueryResponseIndicator = true;
    message.AuthoritativeAnswer = false;
    message.Truncation = false;
    message.RecursionAvailable = false;
    message.ReservedZ = 0;

    message.AddQuestion(new DnsQuestion()
    {
        Name = "codecrafters.io",
        Class = DnsClassType.IN,
        Type = DnsRecordType.A
    });
    message.AddAnswer(new DnsAnswer()
    {
        Name = "codecrafters.io",
        Class = DnsClassType.IN,
        Type = DnsRecordType.A,
        TTL = 60,
        Data = "8.8.8.8"
    });

    byte[] response = message.GetBytes();

    udpClient.Send(response, response.Length, sourceEndPoint);
}

