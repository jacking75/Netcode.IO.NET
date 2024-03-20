using NetcodeIO.NET.Utils.IO;
using NetcodeIO.NET.Utils;

namespace NetcodeIO.NET;

internal struct NetcodePublicConnectToken
{
    public ulong ProtocolID;
    public ulong CreateTimestamp;
    public ulong ExpireTimestamp;
    public ulong ConnectTokenSequence;
    public byte[] PrivateConnectTokenBytes;
    public ConnectTokenServerEntry[] ConnectServers;
    public byte[] ClientToServerKey;
    public byte[] ServerToClientKey;
    public int TimeoutSeconds;

    public bool Read(ByteArrayReaderWriter reader)
    {
        char[] vInfo = new char[13];
        reader.ReadASCIICharsIntoBuffer(vInfo, 13);
        if (!MiscUtils.MatchChars(vInfo, Defines.NETCODE_VERSION_INFO_STR))
            return false;

        ProtocolID = reader.ReadUInt64();

        CreateTimestamp = reader.ReadUInt64();
        ExpireTimestamp = reader.ReadUInt64();
        ConnectTokenSequence = reader.ReadUInt64();
        PrivateConnectTokenBytes = reader.ReadBytes(Defines.NETCODE_CONNECT_TOKEN_PRIVATE_BYTES);
        TimeoutSeconds = (int)reader.ReadUInt32();

        int numServers = (int)reader.ReadUInt32();
        if (numServers < 1 || numServers > Defines.MAX_SERVER_ADDRESSES)
            return false;

        this.ConnectServers = new ConnectTokenServerEntry[numServers];
        for (int i = 0; i < numServers; i++)
        {
            this.ConnectServers[i] = new ConnectTokenServerEntry();
            this.ConnectServers[i].ReadData(reader);
        }

        ClientToServerKey = reader.ReadBytes(32);
        ServerToClientKey = reader.ReadBytes(32);

        return true;
    }

    public void Write(ByteArrayReaderWriter writer)
    {
        writer.WriteASCII(Defines.NETCODE_VERSION_INFO_STR);
        writer.Write(ProtocolID);

        writer.Write(CreateTimestamp);
        writer.Write(ExpireTimestamp);
        writer.Write(ConnectTokenSequence);
        writer.Write(PrivateConnectTokenBytes);
        writer.Write((uint)TimeoutSeconds);

        writer.Write((uint)ConnectServers.Length);
        for (int i = 0; i < ConnectServers.Length; i++)
            ConnectServers[i].WriteData(writer);

        writer.Write(ClientToServerKey);
        writer.Write(ServerToClientKey);
    }
}