using NetcodeIO.NET.Internal;
using NetcodeIO.NET.Utils;
using NetcodeIO.NET.Utils.IO;


namespace NetcodeIO.NET;

internal struct NetcodePrivateConnectToken
{
    public ulong ClientID;
    public int TimeoutSeconds;
    public ConnectTokenServerEntry[] ConnectServers;
    public byte[] ClientToServerKey;
    public byte[] ServerToClientKey;
    public byte[] UserData;

    public bool Read(byte[] token, byte[] key, ulong protocolID, ulong expiration, ulong sequence)
    {
        byte[] tokenBuffer = BufferPool.GetBuffer(Defines.NETCODE_CONNECT_TOKEN_PRIVATE_BYTES);
        int tokenLen = 0;
        try
        {
            tokenLen = PacketIO.DecryptPrivateConnectToken(token, protocolID, expiration, sequence, key, tokenBuffer);
        }
        catch
        {
            BufferPool.ReturnBuffer(tokenBuffer);
            return false;
        }

        try
        {
            using (var reader = ByteArrayReaderWriter.Get(tokenBuffer))
            {
                this.ClientID = reader.ReadUInt64();
                this.TimeoutSeconds = (int)reader.ReadUInt32();
                uint numServerAddresses = reader.ReadUInt32();

                if (numServerAddresses == 0 || numServerAddresses > Defines.MAX_SERVER_ADDRESSES)
                {
                    BufferPool.ReturnBuffer(tokenBuffer);
                    return false;
                }

                this.ConnectServers = new ConnectTokenServerEntry[numServerAddresses];
                for (int i = 0; i < numServerAddresses; i++)
                {
                    this.ConnectServers[i] = new ConnectTokenServerEntry();
                    this.ConnectServers[i].ReadData(reader);
                }

                ClientToServerKey = new byte[32];
                ServerToClientKey = new byte[32];
                UserData = new byte[256];

                reader.ReadBytesIntoBuffer(ClientToServerKey, 32);
                reader.ReadBytesIntoBuffer(ServerToClientKey, 32);
                reader.ReadBytesIntoBuffer(UserData, 256);
            }
        }
        catch
        {
            BufferPool.ReturnBuffer(tokenBuffer);
            return false;
        }

        return true;
    }

    public void Write(ByteArrayReaderWriter stream)
    {
        stream.Write(ClientID);
        stream.Write((uint)TimeoutSeconds);
        stream.Write((uint)ConnectServers.Length);
        foreach (var server in ConnectServers)
            server.WriteData(stream);

        stream.Write(ClientToServerKey);
        stream.Write(ServerToClientKey);
        stream.Write(UserData);
    }
}
