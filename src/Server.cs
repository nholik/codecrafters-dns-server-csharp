using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

var cliArgs = Environment.GetCommandLineArgs();

var useForwarder = (cliArgs.Length > 2 && cliArgs[1] == "--resolver");// || true;
IPEndPoint? forwarderEndpoint = null;

if (useForwarder)
{
    //forwarderEndpoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 53);
    var forwarderAddressParts = cliArgs[2].Split(":");
    if (forwarderAddressParts.Length == 2)
    {
        var forwarderIp = IPAddress.Parse(forwarderAddressParts[0]);
        var forwarderPort = int.Parse(forwarderAddressParts[1]);
        forwarderEndpoint = new IPEndPoint(forwarderIp, forwarderPort);
    }
}

IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
int port = 2053;
IPEndPoint udpEndPoint = new IPEndPoint(ipAddress, port);

UdpClient udpClient = new UdpClient(udpEndPoint);
UdpClient forwarder = new UdpClient();

while (true)
{
    // Receive data
    IPEndPoint sourceEndPoint = new IPEndPoint(IPAddress.Any, 0);
    byte[] receivedData = udpClient.Receive(ref sourceEndPoint);
    string receivedString = Encoding.ASCII.GetString(receivedData);

    var message = new DnsMessage(receivedData[0..3])
    {
        QueryResponseIndicator = true,
        AuthoritativeAnswer = false,
        Truncation = false,
        RecursionAvailable = false,
        ReservedZ = 0
    };

    if (useForwarder)
    {
        Console.WriteLine("using forwarder");

        foreach (var sourceQ in DnsQuestion.Create(receivedData))
        {
            var forwardMessage = new DnsMessage(receivedData[0..3]);
            forwardMessage.AddQuestion(sourceQ);
            var forwardMessageBytes = forwardMessage.GetBytes();

            await forwarder.SendAsync(forwardMessageBytes, forwardMessageBytes.Length, forwarderEndpoint);

            var forwardRecievedResult = await forwarder.ReceiveAsync();

            foreach (var recQ in DnsQuestion.Create(forwardRecievedResult.Buffer))
            {
                message.AddQuestion(recQ);
                message.AddAnswer(new DnsAnswer(forwardRecievedResult.Buffer, recQ.EndingOffset));
            }
        }

        byte[] response = message.GetBytes();

        udpClient.Send(response, response.Length, sourceEndPoint);
    }
    else
    {

        foreach (var q in DnsQuestion.Create(receivedData))
        {
            message.AddQuestion(q);
            message.AddAnswer(new DnsAnswer()
            {
                Name = q.Name,
                Class = q.Class,
                Type = q.Type,
                TTL = 60,
                Data = "8.8.8.8"
            });
        }

        byte[] response = message.GetBytes();

        udpClient.Send(response, response.Length, sourceEndPoint);
    }
}

