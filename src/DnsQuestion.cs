using System.Text;
using System.Buffers.Binary;

public class DnsQuestion
{
    private List<int> _offsets = new List<int>();
    private List<string> _nameParts = new List<string>();
    public string Name { get; init; }
    public DnsRecordType Type { get; set; }
    public DnsClassType Class { get; set; }

    private int _endingOffset;

    public int EndingOffset => _endingOffset;

    public DnsQuestion(byte[] input, int initialOffset)
    {
        var offset = initialOffset;
        var length = input[offset];

        do
        {
            var isCompressed = length == 0xC0;

            if (isCompressed)
            {
                offset++;
                var q = new DnsQuestion(input, input[offset]);
                _nameParts.AddRange(q._nameParts);
                length = 0;
            }
            else
            {
                offset++;
                var nameBytes = input[offset..(offset + length)];
                offset += length;
                length = input[offset];
                _nameParts.Add(Encoding.ASCII.GetString(nameBytes));
            }

        } while (length != 0);

        Name = string.Join(".", _nameParts) ?? string.Empty;
        offset++;

        BinaryPrimitives.TryReadUInt16BigEndian(input[offset..(offset + 2)], out var questionType);
        Type = (DnsRecordType)questionType;
        BinaryPrimitives.TryReadUInt16BigEndian(input[(offset + 2)..(offset + 4)], out var classType);
        Class = (DnsClassType)classType;
        _endingOffset = offset + 4;
    }

    public byte[] GetBytes()
    {
        var questionBytes = Name.ToAddressByteEncoding();
        questionBytes.AddRange(DnsUtils.EncodeRecordType(Type));
        questionBytes.AddRange(DnsUtils.EncodeClass(Class));

        return questionBytes.ToArray();
    }

    public static IEnumerable<DnsQuestion> Create(byte[] input)
    {

        var questionCount = BinaryPrimitives.ReadInt16BigEndian(input[4..6]);
        var result = new List<DnsQuestion>();
        var root = new DnsQuestion(input, 12);
        result.Add(root);

        var nextOffset = root._endingOffset;
        while (result.Count < questionCount && nextOffset < input.Length)
        {
            var q = new DnsQuestion(input, nextOffset);
            result.Add(q);
            nextOffset += q._endingOffset;
        }

        return result;
    }
}