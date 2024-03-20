using NetcodeIO.NET.Utils;
using NetcodeIO.NET.Utils.IO;
using NetcodeIO.NET.Internal;


namespace NetcodeIO.NET;

internal struct NetcodeKeepAlivePacket
{
    public NetcodePacketHeader Header;
    public uint ClientIndex;
    public uint MaxSlots;

    public bool Read(ByteArrayReaderWriter stream, int length, byte[] key, ulong protocolID)
    {
        if (length != 8 + Defines.MAC_SIZE)
            return false;

        byte[] tempBuffer = BufferPool.GetBuffer(length);
        try
        {
            PacketIO.ReadPacketData(Header, stream, length, protocolID, key, tempBuffer);
        }
        catch
        {
            BufferPool.ReturnBuffer(tempBuffer);
            return false;
        }

        using (var dataReader = ByteArrayReaderWriter.Get(tempBuffer))
        {
            ClientIndex = dataReader.ReadUInt32();
            MaxSlots = dataReader.ReadUInt32();
        }

        return true;
    }

    public void Write(ByteArrayReaderWriter stream)
    {
        stream.Write(ClientIndex);
        stream.Write(MaxSlots);
    }
}
