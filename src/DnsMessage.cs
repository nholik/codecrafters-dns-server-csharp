using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlTypes;

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

    public ushort QuestionCount
    {
        get => (ushort)_questions.Count;
        set
        {
            _header[4] = (byte)(value >> 8);
            _header[5] = (byte)(value & 0x00FF);
        }
    }
    public ushort AnswerRecordCount
    {
        get => (ushort)_answers.Count;
        set
        {
            _header[6] = (byte)(value >> 8);
            _header[7] = (byte)(value & 0x00FF);
        }
    }
    public ushort AuthorityRecordCount { get; set; }

    public ushort AdditionalRecordCount { get; set; }

    private readonly List<DnsQuestion> _questions;
    public ReadOnlyCollection<DnsQuestion> Questions => _questions.AsReadOnly();

    private readonly List<DnsAnswer> _answers;

    public ReadOnlyCollection<DnsAnswer> Answers => _answers.AsReadOnly();


    public DnsMessage() : this(new byte[12])
    {
    }

    public DnsMessage(byte[] headerData)
    {
        _header = headerData;
        _questions = new List<DnsQuestion>();
        _answers = new List<DnsAnswer>();
    }

    public void AddQuestion(DnsQuestion question)
    {
        _questions.Add(question);
        QuestionCount = (ushort)_questions.Count;
    }

    public void AddAnswer(DnsAnswer answer)
    {
        _answers.Add(answer);
        AnswerRecordCount = (ushort)_answers.Count;
    }

    public byte[] GetBytes()
    {
        var headerAndQuestionBytes = Questions.Aggregate(_header, (acc, curr) =>
        {
            return acc.Concat(curr.GetBytes()).ToArray();
        });

        var outputeBytes = Answers.Aggregate(headerAndQuestionBytes, (acc, curr) =>
        {
            return acc.Concat(curr.GetBytes()).ToArray();
        });

        return outputeBytes;
    }
}