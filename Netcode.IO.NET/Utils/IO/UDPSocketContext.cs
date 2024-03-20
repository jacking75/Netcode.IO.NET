using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace NetcodeIO.NET.Utils.IO;

internal class UDPSocketContext : ISocketContext
{
    public int BoundPort
    {
        get
        {
            return ((IPEndPoint)internalSocket.LocalEndPoint).Port;
        }
    }

    private Socket internalSocket;
    private Thread socketThread;

    private DatagramQueue datagramQueue;

    public UDPSocketContext(AddressFamily addressFamily)
    {
        datagramQueue = new DatagramQueue(addressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any);
        internalSocket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
    }

    public void Bind(EndPoint endpoint)
    {
        internalSocket.Bind(endpoint);

        socketThread = new Thread(runSocket);
        socketThread.Start();
    }

    public void SendTo(byte[] data, EndPoint remoteEP)
    {
        internalSocket.SendTo(data, remoteEP);
    }

    public void SendTo(byte[] data, int length, EndPoint remoteEP)
    {
        internalSocket.SendTo(data, length, SocketFlags.None, remoteEP);
    }

    public void Pump()
    {
    }

    public bool Read(out Datagram packet)
    {
        if (datagramQueue.Count > 0)
        {
            packet = datagramQueue.Dequeue();
            return true;
        }

        packet = new Datagram();
        return false;
    }

    public void Close()
    {
        internalSocket.Close();
    }

    public void Dispose()
    {
        Close();
    }

    private void runSocket()
    {
        while (true)
        {
            try
            {
                datagramQueue.ReadFrom(internalSocket);
            }
            catch (Exception e)
            {
                if (e is SocketException)
                {
                    var socketException = e as SocketException;
                    if (socketException.SocketErrorCode == SocketError.ConnectionReset) continue;
                }
                return;
            }
        }
    }
}
