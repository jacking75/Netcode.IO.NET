using System;
using System.Net;
using System.Threading;

using NetcodeIO.NET;
using NetcodeIO.NET.Utils.IO;


namespace Test_Server
{
	class Program
	{
		static readonly byte[] _privateKey = new byte[]
		{
			0x60, 0x6a, 0xbe, 0x6e, 0xc9, 0x19, 0x10, 0xea,
			0x9a, 0x65, 0x62, 0xf6, 0x6f, 0x2b, 0x30, 0xe4,
			0x43, 0x71, 0xd6, 0x2c, 0xd1, 0x99, 0x27, 0x26,
			0x6b, 0x3c, 0x60, 0xf4, 0xb7, 0x15, 0xab, 0xa1,
		};

        const Int64 _protocolId = 0x1122334455667788L;
		const Int32 _port = 40000;
        
        static void Main(string[] args)
		{
			startServer();

			Console.ReadLine();
		}
						
		private static void startServer()
		{
			Server server = new Server(
				256,
				"127.0.0.1", _port,
				0x1122334455667788L,
				_privateKey
				);

			server.LogLevel = NetcodeLogLevel.Debug;
			
			server.Start();
			
			Console.WriteLine("Server 시작 성공");


			server.OnClientConnected += Server_OnClientConnected;
			server.OnClientDisconnected += Server_OnClientDisconnected;
			server.OnClientMessageReceived += Server_OnClientMessageReceived;

            Console.WriteLine("key를 누르면 종료");
            Console.ReadLine();

			server.Stop();

            Console.WriteLine("Server 종료");
        }
		
		private static void Server_OnClientMessageReceived(RemoteClient sender, byte[] payload, int payloadSize)
		{
			Console.WriteLine("Received message from " + sender.ClientID);

			// just send it back to them
			sender.SendPayload(payload, payloadSize);
		}

		private static void Server_OnClientDisconnected(RemoteClient client)
		{
			Console.WriteLine("Client " + client.ClientID + " disconnected");
		}

		private static void Server_OnClientConnected(RemoteClient client)
		{
			Console.WriteLine("Client " + client.ClientID + " connected");
		}
	}
}
