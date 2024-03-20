using NetcodeIO.NET.Utils;
using System;
using System.Net;

namespace NetcodeIO.NET.Utils.IO;

internal interface ISocketContext : IDisposable
{
    int BoundPort { get; }
    void Close();
    void Bind(EndPoint endpoint);
    void SendTo(byte[] data, EndPoint remoteEP);
    void SendTo(byte[] data, int length, EndPoint remoteEP);
    bool Read(out Datagram packet);
    void Pump();
}
