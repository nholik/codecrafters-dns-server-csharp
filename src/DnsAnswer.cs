using System.Buffers.Binary;
using System.Text;

public class DnsAnswer
{
    public string Name { get; set; }
    public DnsRecordType Type { get; set; }
    public DnsClassType Class { get; set; }
    public uint TTL { get; set; }
    public ushort Length { get; private set; }
    private string _data = string.Empty;
    public string Data
    {
        get => _data;
        set
        {
            Length = (ushort)value.ToIpByteEncoding().Count;
            _data = value;
        }
    }

    public DnsAnswer()
    {
        Name = string.Empty;
        Data = string.Empty;
    }

    public DnsAnswer(byte[] source, int initialOffset)
    {
        var length = source[initialOffset];
        var offset = initialOffset;
        var nameParts = new List<string>();

        do
        {
            var isCompressed = length == 0xC0;

            if (isCompressed)
            {
                offset++;
                var q = new DnsQuestion(source, source[offset]);
                nameParts.AddRange(q.Name.Split("."));
                length = 0;
            }
            else
            {
                offset++;
                var nameBytes = source[offset..(offset + length)];
                offset += length;
                length = source[offset];
                nameParts.Add(Encoding.ASCII.GetString(nameBytes));
            }

        } while (length != 0);

        Name = string.Join(".", nameParts);
        offset++;

        BinaryPrimitives.TryReadUInt16BigEndian(source[offset..(offset + 2)], out var answerType);
        offset += 2;

        Type = (DnsRecordType)answerType;
        BinaryPrimitives.TryReadUInt16BigEndian(source[offset..(offset + 2)], out var classType);
        Class = (DnsClassType)classType;
        offset += 2;

        BinaryPrimitives.TryReadUInt32BigEndian(source[offset..(offset + 4)], out var ttl);
        TTL = ttl;
        offset += 4;

        BinaryPrimitives.TryReadUInt16BigEndian(source[offset..(offset + 2)], out var len);
        Length = len;
        offset += 2;

        _data = string.Join(".", source[offset..].Select(x => x.ToString()));

    }

    public byte[] GetBytes()
    {
        var answerBytes = Name.ToAddressByteEncoding();
        answerBytes.AddRange(DnsUtils.EncodeRecordType(Type));
        answerBytes.AddRange(DnsUtils.EncodeClass(Class));
        answerBytes.AddRange(DnsUtils.EncodeTtl(TTL));
        answerBytes.AddRange(DnsUtils.EncodeAnswerLength(Length));
        answerBytes.AddRange(Data.ToIpByteEncoding());
        return answerBytes.ToArray();
    }
}