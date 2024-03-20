using NetcodeIO.NET.Internal;
using NetcodeIO.NET.Utils;
using NetcodeIO.NET.Utils.IO;

namespace NetcodeIO.NET;

internal struct NetcodePayloadPacket
{
    public NetcodePacketHeader Header;
    public byte[] Payload;
    public int Length;

    public bool Read(ByteArrayReaderWriter stream, int length, byte[] key, ulong protocolID)
    {
        Payload = BufferPool.GetBuffer(2048);
        Length = 0;
        try
        {
            Length = PacketIO.ReadPacketData(Header, stream, length, protocolID, key, Payload);
        }
        catch
        {
            BufferPool.ReturnBuffer(Payload);
            return false;
        }

        if (Length < 1 || Length > Defines.MAX_PAYLOAD_SIZE)
        {
            BufferPool.ReturnBuffer(Payload);
            return false;
        }

        return true;
    }

    public void Release()
    {
        BufferPool.ReturnBuffer(Payload);
        Payload = null;
        Length = 0;
    }
}