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
    public byte OperationCode
    {
        get => (byte)((_header[2] & 0x78) >> 3);
        set
        {
            var result = (byte)((value << 4) | (_header[2] & 0x8F));
            _header[2] &= result;
        }
    }
    public bool AuthoritativeAnswer
    {
        get => (_header[2] & 0x04) != 0x00;
        set
        {
            if (value)
            {
                _header[2] |= 4;
            }
            else
            {
                _header[2] &= 0xFB;
            }

        }
    }
    public bool Truncation
    {
        get => (byte)(_header[2] & 0x02) != 0x00;
        set
        {
            if (value)
            {
                _header[2] |= 2;
            }
            else
            {
                _header[2] &= 0xFD;
            }

        }
    }
    public bool RecursionDesired
    {
        get => (byte)(_header[2] & 0x01) != 0x00;
        set
        {
            if (value)
            {
                _header[2] |= 1;
            }
            else
            {
                _header[2] &= 0xFE;
            }
        }
    }
    //third byte
    public bool RecursionAvailable { get; set; }
    public byte ReservedZ { get; set; }
    public byte ResponseCode
    {
        get => (byte)(_header[3] & 0x0F);
        set
        {
            _header[3] |= (byte)(value & 0x0F);
        }
    }

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
        _header = new byte[12];
        for (int i = 0; i < headerData.Length; i++)
        {
            _header[i] = headerData[i];
        }
        if (OperationCode == 0)
        {
            ResponseCode = 0;
        }
        else
        {
            ResponseCode = 4;
        }
        _questions = new List<DnsQuestion>();
        _answers = new List<DnsAnswer>();
    }

    public void AddQuestion(DnsQuestion question)
    {
        _questions.Add(question);
        QuestionCount = (ushort)_questions.Count;
        var answer = new DnsAnswer()
        {
            Name = question.Name,
            Class = question.Class,
            Type = question.Type,
            TTL = 60,
            Data = "8.8.8.8",
        };
        _answers.Add(answer);
        AnswerRecordCount = (ushort)_answers.Count;
    }

    // public void AddAnswer(DnsAnswer answer)
    // {
    //     _answers.Add(answer);
    //     AnswerRecordCount = (ushort)_answers.Count;
    // }

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