using System.Collections;
using System.ComponentModel;

public class DnsMessage
{
    private readonly byte[] _header;
    public ushort PacketIdentifier
    {
        get
        {
            var result = (_header[0] << 8) | _header[1];
            return (ushort)result;
        }
        set
        {
            _header[0] = (byte)(value >> 8);
            _header[1] = (byte)(value & 0x00FF);
        }
    }
    public bool QueryResponseIndicator
    {
        get => (_header[2] & 0x80) != 0x00 ? true : false;

        set
        {
            if (value)
            {
                _header[2] |= 8 << 4;
            }
            else
            {
                _header[2] &= 0x80;
            }
        }
    }
    public byte OperationCode { get; set; }
    public bool AuthoritativeAnswer { get; set; }
    public bool Truncation { get; set; }
    public bool RecursionDesired { get; set; }
    public bool RecursionAvailable { get; set; }
    public byte ReservedZ { get; set; }
    public byte ResponseCode { get; set; }

    public ushort QuestionCount { get; set; }
    public ushort AnswerRecordCount { get; set; }
    public ushort AuthorityRecordCount { get; set; }

    public ushort AdditionalRecordCount { get; set; }


    public DnsMessage() : this(new byte[12])
    {
    }

    public DnsMessage(byte[] headerData)
    {
        _header = headerData;
    }

    public byte[] GetBytes() => _header;

}