using System.Text;
using System.Buffers.Binary;

public class DnsQuestion
{
    public required string Name { get; set; }
    public DnsRecordType Type { get; set; }
    public DnsClassType Class { get; set; }

    public DnsQuestion(byte[] input)
    {
        var length = input[0];
        var nameParts = new List<string>();
        var offset = 1;
        do
        {
            var nameBytes = input[offset..(offset + length + 1)];
            offset += length + 1;
            length = input[offset + length + 1];
            nameParts.Add(Encoding.ASCII.GetString(nameBytes));

        } while (length != 0);

        Name = string.Join(".", nameParts);

        BinaryPrimitives.TryReadUInt32BigEndian(new ReadOnlySpan<byte>(input, offset, 2), out var questionType);
        Type = (DnsRecordType)questionType;
        BinaryPrimitives.TryReadUInt32BigEndian(new ReadOnlySpan<byte>(input, offset + 2, 2), out var classType);
        Class = (DnsClassType)classType;
    }

    public DnsQuestion()
    {

    }

    public byte[] GetBytes()
    {
        var questionBytes = Name.ToAddressByteEncoding();
        questionBytes.AddRange(DnsUtils.EncodeRecordType(Type));
        questionBytes.AddRange(DnsUtils.EncodeClass(Class));

        return questionBytes.ToArray();
    }
}