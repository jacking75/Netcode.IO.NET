# Netcode.IO.NET
A pure managed C# implementation of the Netcode.IO spec

아래는 원본 저장소의 글을 한글로 번역한 것이다. 또 fork를 하면서 .NET 8.0 으로 업그레이드 했다.  
`netcode_cpp`에는 원본 netcode 코드를 복사한 것이다.    
  
  
# Project goals
이 프로젝트의 목표는 이식성을 극대화하기 위해 네이티브 DLL이나 래퍼를 전혀 사용하지 않고 .NET 3.5로 코딩된 [Netcode.IO spec](https://github.com/networkprotocol/netcode.io)의 순수 관리형 구현을 제공하는 것입니다.     
이 구현은 원래의 C 참조 구현처럼 libsodium을 사용하는 대신 사용자 정의된 버전의 Bouncy Castle 암호화 라이브러리를 사용합니다. 원본 소스 코드는 여기에서 찾을 수 있습니다: https://github.com/bcgit/bc-csharp  
    
또한 게임에서 사용하도록 설계되었습니다. 이를 위해 처음부터 GC 할당에 최대한 영향을 미치지 않도록 설계되었습니다. 대부분의 경우 Netcode.IO.NET 사용으로 인한 GC 영향은 전혀 나타나지 않을 것입니다.  
    
    
# API usage
Most of the API resides in the namespace `NetcodeIO.NET`

## Server API
To create and start a new server:
```c#
Server server = new Server(
	maxClients,		// int maximum number of clients which can connect to this server at one time
	publicAddress, port,	// string public address and int port clients will connect to
	protocolID,		// ulong protocol ID shared between clients and server
	privateKeyBytes		// byte[32] private crypto key shared between backend servers
);
server.Start();			// start the server running
```

To listen for various events:
```c#
// Called when a client has connected
server.OnClientConnected += clientConnectedHandler;		// void( RemoteClient client )

// Called when a client disconnects
server.OnClientDisconnected += clientDisconnectedHandler;	// void( RemoteClient client )

// 클라이언트로부터 페이로드가 수신되었을 때 호출됩니다.
// 이 호출이 완료되면 페이로드가 풀로 반환되므로 페이로드에 대한 참조를 보관해서는 안 됩니다.
server.OnClientMessageRecieved += messageReceivedHandler;	// void( RemoteClient client, byte[] payload, int payloadSize )

// 서버가 메시지를 기록할 때 호출됩니다.
// 사용자 정의 로거를 사용하지 않는 경우 Console.Write()를 사용하는 핸들러로 충분합니다.
server.OnLogMessage += logMessageHandler;			// void( string message, NetcodeLogLevel logLevel )
```

To send a payload to a remote client connected to the server:
```c#
remoteClient.Send(byte[] payload, int payloadSize);

// or:
server.SendPayload( RemoteClient client, byte[] payload, int payloadSize );
```

To disconnect a client:
```c#
server.Disconnect( RemoteClient client );
```

연결 토큰으로 전달할 수 있는 임의의 256바이트 사용자 데이터를 가져옵니다:
```c#
remoteClient.UserData; // byte[256]
```

To stop a server and disconnect any clients:
```c#
server.Stop();
```

## Client API
To create a new client:
```c#
Client client = new Client();
```

To listen for various events:
```c#
// 클라이언트의 상태가 변경되었을 때 호출됩니다.
// 클라이언트가 서버에 연결되었거나 서버에서 연결이 끊어졌거나 연결 시간이 초과된 경우 등을 감지할 때 사용합니다.
client.OnStateChanged += clientStateChanged;			// void( ClientState state )

// 서버로부터 페이로드가 수신되었을 때 호출됩니다.
// 이 호출이 완료되면 페이로드가 풀로 반환되므로 페이로드에 대한 참조를 보관해서는 안 됩니다.
client.OnMessageReceived += messageReceivedHandler;		// void( byte[] payload, int payloadSize )
```

To connect to a server using a connect token:
```c#
client.Connect( connectToken );		// byte[2048] public connect token as returned by a TokenFactory
```

To send a message to a server when connected:
```c#
client.Send( byte[] payload, int payloadSize );
```

To disconnect a client:
```c#
client.Disconnect();
```

## TokenFactory API
TokenFactory는 클라이언트가 게임 서버에 연결할 때 사용하는 공용 연결 토큰을 생성하는 데 사용할 수 있습니다.  
새 TokenFactory를 생성하려면
```c#
TokenFactory tokenFactory = new TokenFactory(
	protocolID,		// 클라이언트 및 서버 생성자 모두에 전달된 것과 동일한 프로토콜 ID여야 합니다
	privateKey		// byte[32], 는 서버 생성자에 전달된 개인 키와 동일해야 합니다
);
```

To generate a new 2048-byte public connect token:
```c#
tokenFactory.GenerateConnectToken(
	addressList,		// 클라이언트가 연결할 수 있는 주소의 IPEndPoint[] 목록입니다. 하나 이상 32개 이하여야 합니다.
	expirySeconds,		// 토큰이 만료되는 시간(초)
	serverTimeout,		// 연결 시도가 시간 초과되어 클라이언트가 다음 서버를 시도할 때까지 걸리는 시간.
	sequenceNumber,		// 연결 토큰을 고유하게 식별하는 데 사용되는 ulong 토큰 시퀀스 번호.
	clientID,		// 이 클라이언트를 고유하게 식별하는 데 사용되는 ulong ID
	userData		// byte[], 최대 256바이트의 임의의 사용자 데이터(서버에서 RemoteClient.UserData로 사용 가능)
);
```

# A note about UDP and unreliability
Netcode.IO.NET은 Netcode.IO 프로토콜의 순수 포트일 뿐 그 이상도 이하도 아닙니다. Netcode.IO의 핵심은 UDP를 기반으로 한 암호화 및 연결 기반 추상화입니다. 그리고 UDP와 마찬가지로 안정성에 대한 보장이 전혀 없습니다. 메시지가 전달되지 않을 수도 있고 순서대로 전달되지 않을 수도 있습니다. 이는 인터넷의 현실입니다. 그렇긴 하지만 모든 게임에는 일종의 신뢰성 계층이 필요합니다. 이를 위해 제가 만든 ReliableNetcode.NET 프로젝트는 게임에 이 기능을 추가하는 데 사용할 수 있는 불가지론적이고 사용하기 쉬운 안정성 계층을 제공합니다.  
