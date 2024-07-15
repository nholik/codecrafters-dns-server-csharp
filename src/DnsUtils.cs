using System.Buffers.Binary;
using System.Diagnostics.Contracts;
using System.Text;

public static class DnsUtils
{
    public static List<byte> ToAddressByteEncoding(this string addressData)
    {
        var encoded = new List<byte>();
        var nameParts = addressData.Split(".");

        foreach (var part in nameParts)
        {
            encoded.Add((byte)part.Length);
            var encodedName = Encoding.ASCII.GetBytes(part);
            encoded.AddRange(encodedName);
        }
        encoded.Add(0);
        return encoded;
    }

    public static List<byte> ToIpByteEncoding(this string ipData)
    {
        var encoded = new List<byte>();
        var ipParts = ipData.Split(".");
        foreach (var part in ipParts)
        {
            byte.TryParse(part, out var ipSection);
            encoded.Add(ipSection);
        }
        return encoded;
    }

    public static byte[] EncodeRecordType(DnsRecordType typeValue) => EncodeTwoByteResult((ushort)typeValue);
    public static byte[] EncodeClass(DnsClassType classValue) => EncodeTwoByteResult((ushort)classValue);

    public static byte[] EncodeTtl(uint ttlValue) => EncodeFourByteResult(ttlValue);

    public static byte[] EncodeAnswerLength(ushort data) => EncodeTwoByteResult(data);

    private static byte[] EncodeFourByteResult(uint data)
    {
        var resultBytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(new Span<byte>(resultBytes), data);
        return resultBytes;
    }

    private static byte[] EncodeTwoByteResult(ushort data)
    {
        var resultBytes = new byte[2];
        BinaryPrimitives.TryWriteUInt16BigEndian(new Span<byte>(resultBytes), data);
        return resultBytes;
    }

}