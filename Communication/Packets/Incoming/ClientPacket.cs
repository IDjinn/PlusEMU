using System;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming;

public class ClientPacket
{
    private byte[] _body;
    private int _pointer = 0;

    public ClientPacket(int messageId, byte[] body)
    {
        Id = messageId;
        _body = body;
    }

    public int Id { get; private set; }

    public int RemainingLength => _body.Length - _pointer;

    public override string ToString() => "[" + Id + "] BODY: " + PlusEnvironment.GetDefaultEncoding().GetString(_body).Replace(Convert.ToChar(0).ToString(), "[0]");


    public byte[] ReadBytes(int bytes)
    {
        if (bytes > RemainingLength)
            bytes = RemainingLength;
        var data = new byte[bytes];
        for (var i = 0; i < bytes; i++)
            data[i] = _body[_pointer++];
        return data;
    }

    public byte[] PlainReadBytes(int bytes)
    {
        if (bytes > RemainingLength)
            bytes = RemainingLength;
        var data = new byte[bytes];
        for (int x = 0, y = _pointer; x < bytes; x++, y++) data[x] = _body[y];
        return data;
    }

    public byte[] ReadFixedValue()
    {
        var len = HabboEncoding.DecodeInt16(ReadBytes(2));
        return ReadBytes(len);
    }

    public string PopString() => PlusEnvironment.GetDefaultEncoding().GetString(ReadFixedValue());

    public bool PopBoolean() => RemainingLength > 0 && _body[_pointer++] == Convert.ToChar(1);

    public int PopInt()
    {
        if (RemainingLength < 1) return 0;
        var data = PlainReadBytes(4);
        var i = HabboEncoding.DecodeInt32(data);
        _pointer += 4;
        return i;
    }
}