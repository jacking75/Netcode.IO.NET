using NetcodeIO.NET.Internal;
using NetcodeIO.NET.Utils.IO;


namespace NetcodeIO.NET;

internal struct NetcodeDisconnectPacket
{
    public NetcodePacketHeader Header;

    public bool Read(ByteArrayReaderWriter stream, int length, byte[] key, ulong protocolID)
    {
        if (length != Defines.MAC_SIZE)
            return false;

        byte[] tempBuffer = BufferPool.GetBuffer(0);
        try
        {
            PacketIO.ReadPacketData(Header, stream, length, protocolID, key, tempBuffer);
        }
        catch
        {
            BufferPool.ReturnBuffer(tempBuffer);
            return false;
        }

        return true;
    }
}
