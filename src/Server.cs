using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

var cliArgs = Environment.GetCommandLineArgs();

var useResolver = cliArgs.Length > 1 && cliArgs[0] == "--resolver";
IPAddress? forwarderAddress = useResolver ? IPAddress.Parse(cliArgs[1]) : null;

IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
int port = 2053;
IPEndPoint udpEndPoint = new IPEndPoint(ipAddress, port);

UdpClient udpClient = new UdpClient(udpEndPoint);

while (true)
{
    // Receive data
    IPEndPoint sourceEndPoint = new IPEndPoint(IPAddress.Any, 0);
    byte[] receivedData = udpClient.Receive(ref sourceEndPoint);
    string receivedString = Encoding.ASCII.GetString(receivedData);

    Console.WriteLine($"Received {receivedData.Length} bytes from {sourceEndPoint}: {receivedString}");

    // StringBuilder sb = new StringBuilder();
    // foreach (byte b in receivedData)
    // {
    //     sb.AppendFormat("\\x{0:X2}", b);
    // }
    // Console.WriteLine("Recieved packet...");
    // Console.WriteLine(sb.ToString());

    var message = new DnsMessage(receivedData[0..3])
    {
        QueryResponseIndicator = true,
        AuthoritativeAnswer = false,
        Truncation = false,
        RecursionAvailable = false,
        ReservedZ = 0
    };

    foreach (var q in DnsQuestion.Create(receivedData))
    {
        message.AddQuestion(q);
    }

    byte[] response = message.GetBytes();

    udpClient.Send(response, response.Length, sourceEndPoint);
}

