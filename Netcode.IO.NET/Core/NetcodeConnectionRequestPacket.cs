using NetcodeIO.NET.Utils.IO;
using NetcodeIO.NET.Utils;


namespace NetcodeIO.NET;

internal struct NetcodeConnectionRequestPacket
{
    public ulong Expiration;
    public ulong TokenSequenceNum;
    public byte[] ConnectTokenBytes;

    public bool Read(ByteArrayReaderWriter stream, int length, ulong protocolID)
    {
        if (length != 13 + 8 + 8 + 8 + Defines.NETCODE_CONNECT_TOKEN_PRIVATE_BYTES)
            return false;

        char[] vInfo = new char[Defines.NETCODE_VERSION_INFO_BYTES];
        stream.ReadASCIICharsIntoBuffer(vInfo, Defines.NETCODE_VERSION_INFO_BYTES);
        if (!MiscUtils.MatchChars(vInfo, Defines.NETCODE_VERSION_INFO_STR))
        {
            return false;
        }

        if (stream.ReadUInt64() != protocolID)
        {
            return false;
        }

        this.Expiration = stream.ReadUInt64();
        this.TokenSequenceNum = stream.ReadUInt64();
        this.ConnectTokenBytes = BufferPool.GetBuffer(Defines.NETCODE_CONNECT_TOKEN_PRIVATE_BYTES);
        stream.ReadBytesIntoBuffer(this.ConnectTokenBytes, Defines.NETCODE_CONNECT_TOKEN_PRIVATE_BYTES);

        return true;
    }

    public void Release()
    {
        BufferPool.ReturnBuffer(this.ConnectTokenBytes);
    }
}
