using NetcodeIO.NET.Internal;
using NetcodeIO.NET.Utils;
using NetcodeIO.NET.Utils.IO;


namespace NetcodeIO.NET;

internal struct NetcodeChallengeToken
{
    public ulong ClientID;
    public byte[] UserData;

    public bool Read(byte[] token, ulong sequenceNum, byte[] key)
    {
        byte[] tokenBuffer = BufferPool.GetBuffer(300);
        int tokenLen = 0;
        try
        {
            tokenLen = PacketIO.DecryptChallengeToken(sequenceNum, token, key, tokenBuffer);
        }
        catch
        {
            BufferPool.ReturnBuffer(tokenBuffer);
            return false;
        }

        using (var reader = ByteArrayReaderWriter.Get(tokenBuffer))
        {
            ClientID = reader.ReadUInt64();
            UserData = reader.ReadBytes(256);
        }

        return true;
    }

    public void Write(ByteArrayReaderWriter stream)
    {
        stream.Write(ClientID);
        stream.Write(UserData);
    }
}
