using NetcodeIO.NET.Internal;
using NetcodeIO.NET.Utils;
using NetcodeIO.NET.Utils.IO;


namespace NetcodeIO.NET;

internal struct NetcodeConnectionChallengeResponsePacket
{
    public NetcodePacketHeader Header;
    public ulong ChallengeTokenSequence;
    public byte[] ChallengeTokenBytes;

    public bool Read(ByteArrayReaderWriter stream, int length, byte[] key, ulong protocolID)
    {
        byte[] packetBuffer = BufferPool.GetBuffer(8 + 300 + Defines.MAC_SIZE);
        int packetLen = 0;
        try
        {
            packetLen = PacketIO.ReadPacketData(Header, stream, length, protocolID, key, packetBuffer);
        }
        catch (System.Exception e)
        {
            BufferPool.ReturnBuffer(packetBuffer);
            return false;
        }

        if (packetLen != 308)
        {
            BufferPool.ReturnBuffer(packetBuffer);
            return false;
        }

        ChallengeTokenBytes = BufferPool.GetBuffer(300);
        using (var reader = ByteArrayReaderWriter.Get(packetBuffer))
        {
            ChallengeTokenSequence = reader.ReadUInt64();
            reader.ReadBytesIntoBuffer(ChallengeTokenBytes, 300);
        }

        BufferPool.ReturnBuffer(packetBuffer);
        return true;
    }

    public void Write(ByteArrayReaderWriter stream)
    {
        stream.Write(ChallengeTokenSequence);
        stream.Write(ChallengeTokenBytes);
    }

    public void Release()
    {
        BufferPool.ReturnBuffer(ChallengeTokenBytes);
    }
}
