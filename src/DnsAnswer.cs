public class DnsAnswer
{
    public required string Name { get; set; }
    public DnsRecordType Type { get; set; }
    public DnsClassType Class { get; set; }
    public uint TTL { get; set; }
    public ushort Length { get; private set; }
    private string _data = String.Empty;
    public required string Data
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