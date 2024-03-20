using NetcodeIO.NET.Utils.IO;


namespace NetcodeIO.NET;

/// <summary>
	/// Header for a netcode.io packet
	/// </summary>
	internal struct NetcodePacketHeader
{
    public NetcodePacketType PacketType;
    public ulong SequenceNumber;
    public byte ReadSequenceByte;

    /// <summary>
    /// Create the prefix byte for this packet header
    /// </summary>
    public byte GetPrefixByte()
    {
        if (this.PacketType == NetcodePacketType.ConnectionRequest)
        {
            return 0;
        }
        else
        {
            byte prefixByte = 0;
            prefixByte |= (byte)this.PacketType;

            // check how many bytes are required to write sequence number
            int sequenceBytes = 0;
            ulong tempSequenceNumber = this.SequenceNumber;
            while (tempSequenceNumber > 0)
            {
                sequenceBytes++;
                tempSequenceNumber >>= 8;
            }

            if (sequenceBytes == 0)
                sequenceBytes = 1;

            prefixByte |= (byte)(sequenceBytes << 4);

            return prefixByte;
        }
    }

    /// <summary>
    /// Reads a packet from the stream.
    /// If packet is a connection request packet, stream read position lies at version info
    /// Otherwise, stream read position lies at packet-specific data
    /// </summary>
    public void Read(ByteArrayReaderWriter stream)
    {
        byte prefixByte = stream.ReadByte();
        this.ReadSequenceByte = prefixByte;

        // read in packet type
        int packetTypeNibble = prefixByte & 0x0F;
        if (packetTypeNibble >= 7)
        {
            this.PacketType = NetcodePacketType.InvalidPacket;
            return;
        }
        else
        {
            this.PacketType = (NetcodePacketType)packetTypeNibble;
        }

        // read in the sequence number
        // high 4 bits of prefix byte are number of bytes used to encode sequence number
        if (this.PacketType != NetcodePacketType.ConnectionRequest)
        {
            int numSequenceBytes = (prefixByte >> 4);

            // num sequence bytes is between 1 and 8.
            // if it is outside this range, we have an invalid packet
            if (numSequenceBytes < 1 || numSequenceBytes > 8)
            {
                this.PacketType = NetcodePacketType.InvalidPacket;
                return;
            }

            ulong sequenceNumber = 0;
            for (int i = 0; i < numSequenceBytes; i++)
            {
                sequenceNumber |= ((ulong)stream.ReadByte() << (i * 8));
            }

            this.SequenceNumber = sequenceNumber;
        }
    }

    /// <summary>
    /// Writes packet header to the stream
    /// </summary>
    public void Write(ByteArrayReaderWriter stream)
    {
        if (this.PacketType == NetcodePacketType.ConnectionRequest)
        {
            stream.Write((byte)0);
        }
        else
        {
            byte prefixByte = this.GetPrefixByte();

            // now write prefix byte and sequence number bytes
            stream.Write(prefixByte);

            int sequenceBytes = prefixByte >> 4;
            ulong tempSequenceNumber = this.SequenceNumber;
            for (int i = 0; i < sequenceBytes; i++)
            {
                stream.Write((byte)(tempSequenceNumber & 0xFF));
                tempSequenceNumber >>= 8;
            }
        }
    }
}
