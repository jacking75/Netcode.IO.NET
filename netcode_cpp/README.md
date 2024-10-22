[GitHub netcode](https://github.com/mas-bandwidth/netcode ) 저장소의 글을 중심으로 번역, 추가한 글이다.    
2024-10-22    
    
# netcode
**netcode**는 UDP 위에 구축된 보안 클라이언트/서버 프로토콜로 연결 지향 프로토콜이 필요하지만 TCP의 헤드 오브 라인 차단이 없는 실시간 멀티플레이어 게임에서 사용하기 위한 것입니다.  
  
netcode에는 다음과 같은 기능이 있습니다:   
- 커넥트 토큰을 사용한 보안 클라이언트 연결. 승인한 클라이언트만 서버에 연결할 수 있습니다. 이는 웹 백엔드에서 매치메이킹을 수행한 다음 클라이언트를 서버로 보내는 게임에 적합합니다. 
- 클라이언트 슬롯 시스템. 서버에는 클라이언트를 위한 n개의 슬롯이 있습니다. 클라이언트는 서버에 연결할 때 슬롯이 할당되며 모든 슬롯이 사용되면 빠르게 연결이 거부됩니다.
- 클라이언트 또는 서버 측에서 연결을 빠르게 끊어 새 클라이언트를 위한 슬롯을 열고 하드 연결을 위한 타임아웃을 설정합니다. 
- 암호화 및 서명된 패킷. 연결에 관여하지 않은 당사자가 패킷을 변조하거나 읽을 수 없습니다. 암호화는 우수한 나트륨 라이브러리에 의해 수행됩니다. 
- 악의적으로 조작된 패킷, 패킷 재생 공격 및 패킷 증폭 공격에 대한 강력한 보호 등 다양한 보안 기능. 
- Wi-Fi 라우터에서 지터를 크게 줄일 수 있는 패킷 태깅을 지원합니다. 자세한 내용은 [이 문서](https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/networking/overviews/qos-packet-tagging )를 참조하세요.   
  
netcode는 안정적이며 프로덕션 준비가 완료되었습니다.  
   
  
 ## 사용법 
무작위 32바이트의 개인키를 생성하는 것으로 시작합니다.  개인 키를 다른 사람과 공유하지 마세요.  
특히 **클라이언트 실행 파일에 개인 키를 포함시키지 마세요!**     
  
다음은 테스트 개인 키입니다:  
```
static uint8_t private_key[NETCODE_KEY_BYTES] = { 0x60, 0x6a, 0xbe, 0x6e, 0xc9, 0x19, 0x10, 0xea, 
                                                  0x9a, 0x65, 0x62, 0xf6, 0x6f, 0x2b, 0x30, 0xe4, 
                                                  0x43, 0x71, 0xd6, 0x2c, 0xd1, 0x99, 0x27, 0x26,
                                                  0x6b, 0x3c, 0x60, 0xf4, 0xb7, 0x15, 0xab, 0xa1 };
```  
  
개인 키로 서버를 만듭니다:      
```
char * server_address = "127.0.0.1:40000";

struct netcode_server_config_t server_config;
netcode_default_server_config( &server_config );
memcpy( &server_config.private_key, private_key, NETCODE_KEY_BYTES );

struct netcode_server_t * server = netcode_server_create( server_address, &server_config, time );
if ( !server )
{
    printf( "error: failed to create server\n" );
    return 1;
}
```  
    
그런 다음 원하는 수의 클라이언트 슬롯으로 서버를 시작합니다:    
```
netcode_server_start( server, 16 );
```    
  
클라이언트를 연결하려면 클라이언트가 연결 토큰을 반환하는 백엔드에 REST API를 호출해야 합니다.  
연결 토큰을 사용하면 백엔드로 인증된 클라이언트만 연결할 수 있도록 서버를 보호할 수 있습니다.    
```
netcode_client_connect( client, connect_token );
```  
    
클라이언트가 서버에 연결되면 클라이언트에 클라이언트 인덱스가 할당되고 암호화되고 서명된 패킷을 서버와 교환할 수 있습니다.  
자세한 내용은 client.c 및 server.c를 참조하세요.	  
    
    
# netcode 프로토콜의 표준 (STANDARD.md)  
이 문서는 사용자가 직접 구현할 수 있도록 이 프로토콜의 표준을 설명합니다.  
  
## 아키텍처
넷코드 아키텍처에는 세 가지 주요 구성 요소가 있습니다:
1. 백엔드
2. 전용 서버
3. 클라이언트  
  
웹 백엔드는 클라이언트를 인증하고 REST API를 제공하는 일반적인 웹 서버(예: nginx)입니다. 클라이언트는 전용 서버 인스턴스에 연결하려는 넷코드 프로토콜을 실행하는 엔드포인트입니다. 전용 서버는 데이터 센터나 클라우드에서 실행되는 게임 또는 애플리케이션의 서버 측 인스턴스입니다.  
      
클라이언트 연결의 작업 순서는 다음과 같습니다:  
1. 클라이언트가 웹 백엔드로 인증합니다.
2. 인증된 클라이언트가 웹 백엔드에 REST 호출을 통해 게임 플레이를 요청합니다.
3. 웹 백엔드는 연결 토큰을 생성하고 HTTPS를 통해 해당 클라이언트에게 반환합니다.
4. 클라이언트는 연결 토큰을 사용하여 UDP를 통해 전용 서버와 연결을 설정합니다.
5. 전용 서버는 유효한 연결 토큰을 가진 클라이언트만 연결할 수 있도록 로직을 실행합니다.
6. 연결이 설정되면 클라이언트와 서버는 암호화되고 서명된 UDP 패킷을 교환합니다.
  
  
## 일반 규칙
넷코드는 바이너리 프로토콜입니다.  
  
모든 데이터는 달리 명시되지 않는 한 **리틀 엔디안** 바이트 순서로 기록됩니다.   
  
이는 토큰 및 패킷 데이터뿐만 아니라 바이트 배열 논스 값으로 변환된 시퀀스 번호와 AEAD 암호화 프리미티브에 전달된 관련 데이터에도 적용됩니다.  
  
  
## 토큰 연결
연결 토큰은 인증된 클라이언트만 전용 서버에 연결할 수 있도록 합니다.  
    
연결 토큰은 공개와 비공개 두 부분으로 구성됩니다.  
  
비공개 부분은 웹 백엔드와 전용 서버 인스턴스 간에 공유되는 개인 키로 암호화되고 서명됩니다.  
  
암호화하기 전에 비공개 연결 토큰 데이터는 다음과 같은 바이너리 형식을 갖습니다.  
```
[client id] (uint64) // 인증된 클라이언트의 글로벌 고유 식별자
[timeout seconds] (uint32) // timeout(초). 음수 값은 timeout을 비활성화합니다(개발자 전용)
[num server addresses] (uint32) // in [1,32]
<for each server address>
{
    [address type] (uint8) // value of 1 = IPv4 address, 2 = IPv6 address.
    <if IPV4 address>
    {
        // for a given IPv4 address: a.b.c.d:port
        [a] (uint8)
        [b] (uint8)
        [c] (uint8)
        [d] (uint8)
        [port] (uint16)
    }
    <else IPv6 address>
    {
        // for a given IPv6 address: [a:b:c:d:e:f:g:h]:port
        [a] (uint16)
        [b] (uint16)
        [c] (uint16)
        [d] (uint16)
        [e] (uint16)
        [f] (uint16)
        [g] (uint16)
        [h] (uint16)
        [port] (uint16)
    }
}
[client to server key] (32 bytes)
[server to client key] (32 bytes)
[user data] (256 bytes) // user defined data specific to this protocol id
<zero pad to 1024 bytes>
```  
이 데이터는 가변 크기이지만 간단하게 하기 위해 1024바이트의 고정 크기 버퍼에 기록됩니다. 사용하지 않은 바이트는 0으로 채워집니다.  
  
비공개 연결 토큰 데이터의 암호화는 다음 바이너리 데이터를 연관 데이터로 사용하여 libsodium AEAD 프리미티브 crypto_aead_xchacha20poly1305_ietf_encrypt 로 수행됩니다:
```
[version info] (13 bytes)       // "NETCODE 1.02" ASCII with null terminator.
[protocol id] (uint64)          // 64 bit value unique to this particular game/application
[expire timestamp] (uint64)     // 64 bit unix timestamp when this connect token expires
```  
  
암호화에 사용되는 논스는 모든 토큰에 대해 무작위로 생성되는 24바이트 숫자입니다.  
  
암호화는 버퍼의 처음 1024바이트에서 16바이트까지 수행되며, 마지막 16바이트는 [HMAC](https://velog.io/@stop7089/HMAC-%EC%9D%B4%EB%9E%80 )을 저장하기 위해 남겨집니다:  
```
[encrypted private connect token] (1008 bytes)
[hmac of encrypted private connect token] (16 bytes)
```  
  
암호화 후 이를 암호화된 비공개 연결 토큰 데이터라고 합니다.    
  
공개 데이터와 비공개 데이터가 함께 연결 토큰을 형성합니다:  
```
[version info] (13 bytes)       // "NETCODE 1.02" ASCII with null terminator.
[protocol id] (uint64)          // 64 bit value unique to this particular game/application
[create timestamp] (uint64)     // 64 bit unix timestamp when this connect token was created
[expire timestamp] (uint64)     // 64 bit unix timestamp when this connect token expires
[connect token nonce] (24 bytes)
[encrypted private connect token data] (1024 bytes)
[timeout seconds] (uint32)      // timeout in seconds. negative values disable timeout (dev only)
[num_server_addresses] (uint32) // in [1,32]
<for each server address>
{
    [address_type] (uint8) // value of 1 = IPv4 address, 2 = IPv6 address.
    <if IPV4 address>
    {
        // for a given IPv4 address: a.b.c.d:port
        [a] (uint8)
        [b] (uint8)
        [c] (uint8)
        [d] (uint8)
        [port] (uint16)
    }
    <else IPv6 address>
    {
        // for a given IPv6 address: [a:b:c:d:e:f:g:h]:port
        [a] (uint16)
        [b] (uint16)
        [c] (uint16)
        [d] (uint16)
        [e] (uint16)
        [f] (uint16)
        [g] (uint16)
        [h] (uint16)
        [port] (uint16)
    }
}
[client to server key] (32 bytes)
[server to client key] (32 bytes)
<zero pad to 2048 bytes>
```      
이 데이터는 가변 크기이지만 간단하게 하기 위해 2048 바이트의 고정 크기 버퍼에 기록됩니다. 사용하지 않은 바이트는 0으로 채워집니다.  
  
  
## 챌린지 토큰
챌린지 토큰은 스푸핑된 IP 패킷 소스 주소를 가진 클라이언트가 서버에 연결하지 못하도록 차단합니다.  
  
암호화 이전 챌린지 토큰의 구조는 다음과 같습니다:  
```
[client id] (uint64)
[user data] (256 bytes)
<zero pad to 300 bytes>
```  
챌린지 토큰 데이터의 암호화는 관련 데이터가 없는 libsodium AEAD 프리미티브 crypto_aead_chacha20poly1305_ietf_encrypt, 전용 서버가 시작될 때 생성되는 랜덤 키, 0에서 시작하여 챌린지 토큰이 생성될 때마다 증가하는 시퀀스 번호로 수행됩니다. 시퀀스 번호는 높은 비트를 0으로 채워 96비트 논스를 생성하여 확장합니다.  
  
암호화는 처음 300~16바이트에서 수행되며, 마지막 16바이트는 암호화된 버퍼의 HMAC을 저장합니다:  
```
[encrypted challenge token] (284 bytes)
[hmac of encrypted challenge token data] (16 bytes)
```  
  
이를 암호화된 챌린지 토큰 데이터라고 합니다.  
  
  
## 패킷
넷코드에는 다음과 같은 패킷이 있습니다:  
- 연결 요청 패킷 (0)
- 연결 거부 패킷 (1)
- 연결 챌린지 패킷 (2)
- 연결 응답 패킷 (3)
- 연결 유지 패킷 (4)
- 연결 페이로드 패킷 (5)
- 연결 끊기 패킷 (6)
  
첫 번째 패킷 유형 연결 요청 패킷 (0)은 암호화되지 않았으며 다음 형식을 갖습니다:  
```
0 (uint8) // prefix byte of zero
[version info] (13 bytes)       // "NETCODE 1.02" ASCII with null terminator.
[protocol id] (8 bytes)
[connect token expire timestamp] (8 bytes)
[connect token nonce] (24 bytes)
[encrypted private connect token data] (1024 bytes)
```  
  
다른 모든 패킷 유형은 암호화됩니다.  
  
암호화 이전에는 패킷 유형 `>= 1` 의 형식이 다음과 같습니다:    
```
[prefix byte] (uint8) // non-zero prefix byte
[sequence number] (variable length 1-8 bytes)
[per-packet type data] (variable length according to packet type)
```  

접두사 바이트의 하위 4비트에 패킷 유형이 포함됩니다.  
상위 4비트에는 [1,8] 범위의 시퀀스 번호에 대한 바이트 수가 포함됩니다.  
  
시퀀스 번호는 상위 0 바이트를 생략하여 인코딩됩니다. 예를 들어 시퀀스 번호 1000은 0x000003E8이며 해당 값을 전송하는 데 2바이트만 필요합니다. 따라서 접두사 바이트의 상위 4비트는 2로 설정되고 패킷에 기록되는 시퀀스 데이터는 다음과 같습니다:  
<pre>
0xE8,0x03
</pre>  
  
이렇게 패킷에 기록된 시퀀스 번호 바이트는 반전된 것 입니다:  
```
<for each sequence byte written>
{
    write_byte( sequence_number & 0xFF )
    sequence_number >>= 8
}
```  
  
시퀀스 번호 뒤에는 패킷 유형별 데이터가옵니다:
  
연결이 거부된 패킷:  
```
<no data>
```  
  
연결 챌린지 패킷:  
```
[challenge token sequence] (uint64)
[encrypted challenge token data] (300 bytes)
```  
  
  
연결 응답 패킷:  
````
[challenge token sequence] (uint64)
[encrypted challenge token data] (300 bytes)
```  
  
  
연결 유지 패킷:  
```
[client index] (uint32)
[max clients] (uint32)
```  
  
   
연결 페이로드 패킷:  
```
[payload data] (1 to 1200 bytes)
```  
  
    
연결 끊기 패킷:  
```
<no data>
```  
  
패킷별 데이터는 다음 바이너리 데이터를 연관 데이터로 사용하여 libsodium AEAD 프리미티브 `crypto_aead_chacha20poly1305_ietf_encrypt` 를 사용하여 암호화합니다:  
```
[version info] (13 bytes)       // "NETCODE 1.02" ASCII with null terminator.
[protocol id] (uint64)          // 64 bit value unique to this particular game/application
[prefix byte] (uint8)           // prefix byte in packet. stops an attacker from modifying packet type.
```  
  
패킷 시퀀스 번호는 높은 비트를 0으로 채워 96비트 논스를 생성하여 확장됩니다.   
  
클라이언트에서 서버로 전송되는 패킷은 연결 토큰의 클라이언트-서버 키로 암호화됩니다.   
  
서버에서 클라이언트로 전송되는 패킷은 해당 클라이언트에 대한 연결 토큰의 서버 대 클라이언트 키를 사용하여 암호화됩니다.   
  
암호화 후 패킷 유형 `>= 1` 은 다음과 같은 형식을 갖습니다:  
```
[prefix byte] (uint8) // non-zero prefix byte: ( (num_sequence_bytes<<4) | packet_type )
[sequence number] (variable length 1-8 bytes)
[encrypted per-packet type data] (variable length according to packet type)
[hmac of encrypted per-packet type data] (16 bytes)
```  
  
    
## 암호화된 패킷 읽기
암호화된 패킷을 읽을 때는 다음 단계를 정확한 순서대로 수행합니다:    
- 패킷 크기가 18바이트 미만이면 너무 작아서 유효하지 않을 가능성이 있으므로 패킷을 무시합니다.  
- 접두사 바이트의 하위 4비트가 7보다 크거나 같으면 패킷 유형이 유효하지 않으므로 패킷을 무시합니다.
- 서버는 연결 챌린지 패킷 형식의 패킷을 무시합니다.
- 클라이언트는 연결 요청 패킷 유형과 연결 응답 패킷 유형의 패킷을 무시합니다.
- 접두사 바이트의 상위 4비트(시퀀스 바이트)가 [1,8] 범위를 벗어나는 경우 패킷을 무시합니다.
- 패킷 크기가 1 + 시퀀스 바이트 + 16 보다 작으면 유효하지 않을 가능성이 있으므로 패킷을 무시합니다.
- 패킷 유형별 데이터 크기가 패킷 유형에 대한 예상 크기와 일치하지 않으면 패킷을 무시합니다.
    - 접속 거부 패킷의 경우 0바이트.
    - 연결 챌린지 패킷의 경우 308바이트
    - 연결 응답 패킷의 경우 308바이트
    - 연결 유지 패킷의 경우 8바이트
    - 연결 페이로드 패킷의 경우 [1,1200] 바이트
    - 연결 끊기 패킷의 경우 0바이트
- 패킷 유형이 이미 수신된 리플레이 보호 테스트에 실패한 경우 패킷을 무시합니다. 자세한 내용은 아래 재생 보호 섹션을 참조하세요.
- 패킷 유형별 데이터 해독에 실패하면 패킷을 무시하세요.
- 가장 최근의 다시보기 보호 시퀀스 #를 진행합니다. 자세한 내용은 아래의 다시보기 보호 섹션을 참조하세요.
- 위의 모든 검사를 통과하면 패킷이 처리됩니다.
  
  
## 리플레이 보호
재생 보호는 공격자가 프로토콜을 위반하기 위해 유효한 패킷을 녹화하고 나중에 다시 재생하는 것을 차단합니다.  
  
리플레이 보호를 활성화하기 위해 넷코드는 다음을 수행합니다:  
- 암호화된 패킷은 0에서 시작하여 패킷이 전송될 때마다 증가하는 64비트 시퀀스 번호로 전송됩니다.
- 시퀀스 번호는 패킷 헤더에 포함되어 있으며 패킷 수신자가 복호화 전에 읽을 수 있습니다.
- 시퀀스 번호는 패킷 암호화를 위한 논스로 사용되므로 시퀀스 번호를 수정하면 암호화 서명 검사에 실패합니다.
    
	
리플레이 보호 알고리즘은 다음과 같습니다:    
1.  수신된 가장 최근 시퀀스 번호에서 재생 버퍼 크기를 뺀 값보다 오래된 패킷은 수신자 측에서 버려집니다.
2.  가장 최근 시퀀스 번호의 재생 버퍼 크기 내에 있는 패킷이 도착하면 해당 시퀀스 번호가 아직 수신되지 않은 경우에만 허용되고 그렇지 않은 경우 무시됩니다.
3. 패킷이 성공적으로 복호화된 후, a) 패킷 시퀀스 번호가 리플레이 버퍼 창에 있는 경우 해당 항목은 수신된 것으로 설정되고, b) 패킷 시퀀스 번호가 이전에 수신된 가장 최근의 시퀀스 번호보다 <>인 경우 가장 최근의 시퀀스 번호가 업데이트됩니다.  
  
  
리플레이 보호는 클라이언트와 서버 모두에서 다음 패킷 유형에 적용됩니다:  
- 연결 유지 패킷
- 연결 페이로드 패킷
- 연결 끊기 패킷  
  
리플레이 버퍼 크기는 구현에 따라 다르지만, 일반적인 전송 속도(20-60HZ)에서 몇 초 분량의 패킷이 지원되어야 한다는 것을 원칙으로 합니다. 보수적으로 클라이언트당 256개 항목의 리플레이 버퍼 크기는 대부분의 애플리케이션에 충분합니다.  
  
  
## 클라이언트 상태 머신
클라이언트의 상태는 다음과 같습니다:  
- 연결 토큰 만료 (-6)
- 잘못된 연결 토큰 (-5)
- 연결 시간 초과 (-4)
- 연결 응답 시간 초과 (-3)
- 연결 요청 시간 초과 (-2)
- 연결 거부됨 (-1)
- 연결 끊김 (0)
- 연결 요청 전송 중 (1)
- 연결 응답 전송 중 (2)
- 연결됨 (3)  
  
초기 상태는 연결이 끊어진 상태(0)입니다. 음수 상태는 오류 상태를 나타냅니다. 목표 상태는 연결됨 (3) 입니다.   
   
### 연결 토큰 요청
클라이언트가 서버에 연결하려는 경우 웹 백엔드에서 연결 토큰을 요청합니다.  
  
다음 측면은 이 표준의 범위를 벗어납니다:  
1. 클라이언트가 웹 백엔드에서 연결 토큰을 요청하는 데 사용하는 메커니즘.
2. 웹 백엔드가 연결 토큰에 포함할 서버 주소 집합을 결정하는 데 사용하는 메커니즘입니다.  
  
클라이언트가 연결 토큰을 획득하면 연결 토큰에 포함된 서버 주소 중 하나에 연결을 설정하는 것이 목표입니다.  
  
이 프로세스를 시작하기 위해 연결 토큰의 첫 번째 서버 주소로 연결 요청 보내기로 전환합니다.  
  
이 작업을 수행하기 전에 클라이언트는 연결 토큰이 유효한지 확인합니다. 연결 토큰의 서버 주소 수가 [1,32] 범위를 벗어나거나 연결 토큰의 주소 유형 값이 [0,1] 범위를 벗어나거나 생성 타임스탬프가 만료 타임스탬프보다 최근인 경우 클라이언트는 무효 연결 토큰으로 전환합니다.  
  
### 연결 요청 보내기
연결 요청을 보내는 동안 클라이언트는 10HZ와 같은 일정한 속도로 서버에 연결 요청 패킷을 보냅니다.  
  
클라이언트가 서버로부터 연결 챌린지 패킷을 수신하면 챌린지 토큰 데이터를 저장하고 도전 응답 전송으로 전환합니다. 이는 연결 프로세스의 다음 단계로 성공적으로 전환되었음을 나타냅니다.  
   
연결 요청 전송 중에서 다른 모든 전환은 실패 사례입니다. 이러한 경우 클라이언트는 연결 토큰의 다음 서버 주소로 연결을 시도합니다(예: 연결 토큰의 다음 서버 주소로 연결 요청 보내기 상태로 전환). 또는 연결할 추가 서버 주소가 없는 경우 클라이언트는 다음 단락에 설명된 대로 적절한 오류 상태로 전환됩니다.  
  
연결 요청 거부 패킷이 연결 요청 전송 중 수신되면 클라이언트는 접속 거부로 전환됩니다. 연결 토큰에 지정된 시간 초과 기간 내에 연결 챌린지 패킷이나 연결 거부 패킷이 수신되지 않으면 클라이언트는 연결 요청 시간 초과로 전환합니다.  
  
### 챌린지 응답 보내기
챌린지 응답 보내기에서 클라이언트는 10HZ와 같은 일정한 속도로 챌린지 응답 패킷을 서버에 보냅니다.  
  
클라이언트가 서버로부터 연결 유지 패킷을 수신하면 클라이언트 인덱스와 최대 클라이언트를 패킷에 저장하고 연결됨으로 전환합니다.  
  
연결 페이로드 패킷 이전에 수신된 모든 connected 패킷은 버려집니다.  
  
챌린지 응답 전송의 다른 모든 전환은 실패 사례입니다. 이러한 경우 클라이언트는 연결 토큰의 다음 서버 주소로 연결을 시도합니다(예: 연결 토큰의 다음 서버 주소로 연결 요청 보내기로 전환). 또는 연결할 추가 서버 주소가 없는 경우 클라이언트는 다음 단락에 설명된 대로 적절한 오류 상태로 전환합니다.   
  
접속 요청 거부 패킷이 챌린지 응답 전송 중 수신되면 클라이언트는 접속 거부로 전환합니다. 연결 토큰에 지정된 시간 초과 기간 내에 연결 유지 패킷 또는 연결 거부 패킷이 수신되지 않으면 클라이언트는 도전 응답 시간 초과로 전환합니다.  
  
### 연결 토큰 만료
전체 클라이언트 연결 프로세스(여러 서버 주소에 걸쳐 있을 수 있음)가 서버에 성공적으로 연결하기 전에 연결 토큰이 만료될 정도로 오래 걸리면 클라이언트는 연결 토큰 만료됨으로 전환됩니다.  
  
이 시간은 연결 토큰의 생성 타임스탬프에서 만료 타임스탬프를 빼서 결정해야 합니다.  
  
### 커넥티드
연결된 상태에서 클라이언트는 서버로부터 받은 연결 페이로드 패킷을 버퍼링하여 페이로드 데이터를 클라이언트 애플리케이션에 넷코드 패킷으로 전달할 수 있도록 합니다.  
  
연결된 상태에서 클라이언트 애플리케이션은 연결 페이로드 패킷을 서버로 전송할 수 있습니다. 클라이언트 애플리케이션이 보낸 연결 페이로드 패킷이 없는 경우 클라이언트는 10HZ와 같은 일정한 속도로 연결 유지 패킷을 생성하여 서버로 보냅니다.  
  
연결 토큰에 지정된 시간 초과 기간 내에 서버로부터 연결 페이로드 패킷 또는 연결 유지 패킷이 수신되지 않으면 클라이언트는 연결 시간 초과로 전환합니다.  
  
클라이언트가 서버로부터 connected 패킷을 수신하는 동안 connect disconnect 패킷을 수신하면 disconnected로 전환됩니다.  
  
클라이언트가 서버와의 연결을 끊으려는 경우, 연결 끊기 패킷을 여러 번 중복 전송한 후 연결 끊기로 전환합니다.  
  
     
## 서버 측 연결 프로세스  
  
### 서버 측 개요
전용 서버는 공개적으로 액세스할 수 있는 IP 주소와 포트에 있어야 합니다.  
  
서버는 n개의 클라이언트 슬롯 세트를 관리하며, [0,n-1] 의 각 슬롯은 연결된 클라이언트 한 개를 위한 공간을 나타냅니다.  
  
서버당 최대 클라이언트 슬롯 수는 구현에 따라 다릅니다. 일반적인 사용 사례는 [2,64]  범위에서 예상되지만 참조 구현에서는 서버당 최대 256명의 클라이언트를 지원합니다.  
  
구현에서 효율적으로 처리할 수 있는 경우 서버당 더 많은 클라이언트를 지원할 수 있습니다.  
  
### 연결 요청 처리
서버는 연결 요청을 처리할 때 다음과 같은 엄격한 규칙을 따릅니다:  
1. 클라이언트가 연결하려면 유효한 연결 토큰이 있어야 합니다.
2. 꼭 필요한 경우에만 클라이언트에 응답하세요.
3. 잘못된 요청은 최소한의 작업으로 가능한 한 빨리 무시하세요.
4. DDoS 증폭을 방지하기 위해 응답 패킷이 요청 패킷보다 작은지 확인하세요.
  
서버가 클라이언트로부터 연결 요청 패킷을 수신하면 다음 데이터가 포함됩니다:  
```
0 (uint8) // prefix byte of zero
[version info] (13 bytes)       // "NETCODE 1.02" ASCII with null terminator.
[protocol id] (8 bytes)
[connect token expire timestamp] (8 bytes)
[connect token nonce] (24 bytes)
[encrypted private connect token data] (1024 bytes)
```  

그러나 이 패킷은 암호화되지 않습니다:  
- 전용 서버 인스턴스와 웹 백엔드만 암호화된 비공개 연결 토큰 데이터를 읽을 수 있는데, 이는 둘 간에 공유된 개인 키로 암호화되어 있기 때문입니다.
- 버전 정보, 프로토콜 ID, 연결 토큰 만료 타임스탬프와 같은 패킷의 중요한 부분은 AEAD 구조로 보호되므로 서명 검사에 실패하지 않고는 수정할 수 없습니다.  
  
  
서버는 연결 요청 패킷을 처리할 때 다음 단계를 정확한 순서대로 수행합니다:
- 패킷이 예상 크기인 1078바이트가 아닌 경우 패킷을 무시합니다.
- 패킷의 버전 정보가 "NETCODE 1.02"(13바이트, null 터미네이터 포함)와 일치하지 않으면 패킷을 무시합니다.
- 패킷의 프로토콜 ID가 전용 서버의 예상 프로토콜 ID와 일치하지 않으면 패킷을 무시합니다.
- 연결 토큰 만료 타임스탬프가 현재 타임스탬프와 같으면 패킷을 무시합니다.
- 암호화된 비공개 연결 토큰 데이터가 버전 정보, 프로토콜 ID, 만료 타임스탬프에서 생성된 연관 데이터를 사용하여 비공개 키로 해독되지 않으면 패킷을 무시합니다.
- 해독된 비공개 연결 토큰이 예상 범위인 [1,32]를 벗어난 서버 주소 수 또는 [0,1] 범위를 벗어난 주소 유형 값 등 어떤 이유로든 읽지 못하면 패킷을 무시합니다.
- 전용 서버 공개 주소가 비공개 연결 토큰의 서버 주소 목록에 없는 경우 패킷을 무시합니다.
- 패킷 IP 소스 주소와 포트의 클라이언트가 이미 연결되어 있으면 패킷을 무시합니다.
- 비공개 연결 토큰 데이터에 포함된 클라이언트 ID를 가진 클라이언트가 이미 연결되어 있는 경우 패킷을 무시합니다.
- 연결 토큰이 이미 다른 패킷 소스 IP 주소와 포트에서 사용된 경우 패킷을 무시합니다.
- 그렇지 않으면 이미 사용된 연결 토큰 기록에 비공개 연결 토큰 hmac + 패킷 소스 IP 주소 및 포트를 추가합니다.
- 사용 가능한 클라이언트 슬롯이 없으면 서버가 가득 찬 것입니다. 연결 거부 패킷으로 응답합니다.
- 패킷 소스 IP 주소 및 포트에 대한 암호화 매핑을 추가하여 해당 주소 및 포트에서 읽은 패킷은 비공개 연결 토큰의 클라이언트-서버 키를 사용하여 해독되고 해당 주소 및 포트로 전송된 패킷은 비공개 연결 토큰의 서버-클라이언트 키를 사용하여 암호화되도록 합니다. 이 암호화 매핑은 해당 주소 및 포트에서 패킷을 주고받지 않거나 클라이언트가 초 이내에 서버와 연결을 설정하지 못하는 경우 초 후에 만료됩니다.
- 어떤 이유로 이 암호화 매핑을 추가할 수 없는 경우 패킷을 무시하세요.
- 그렇지 않으면 연결 챌린지 패킷으로 응답하고 연결 챌린지 시퀀스 번호를 늘리세요.
  
### 연결 응답 패킷 처리
클라이언트가 서버로부터 연결 챌린지 패킷을 받으면 연결 응답 패킷으로 응답합니다.  
  
연결 응답 패킷에는 다음 데이터가 포함됩니다:  
```
[prefix byte] (uint8) // non-zero prefix byte: ( (num_sequence_bytes<<4) | packet_type )
[sequence number] (variable length 1-8 bytes)
[challenge token sequence] (uint64)
[encrypted challenge token data] (360 bytes)
```  

서버는 연결 응답 패킷을 처리할 때 다음 단계를 정확한 순서대로 수행합니다:  
- 암호화된 챌린지 토큰 데이터의 암호 해독에 실패하면 패킷을 무시합니다.
- 패킷 소스 주소와 포트의 클라이언트가 이미 연결되어 있는 경우 패킷을 무시합니다.
- 암호화된 챌린지 토큰 데이터에 포함된 클라이언트 ID를 가진 클라이언트가 이미 연결되어 있는 경우 패킷을 무시합니다.
- 사용 가능한 클라이언트 슬롯이 없으면 서버가 가득 찬 것입니다. 연결 거부 패킷으로 응답합니다.
- 패킷 IP 주소 + 포트 및 클라이언트 ID를 빈 클라이언트 슬롯에 할당하고 해당 클라이언트를 연결됨으로 표시합니다.
- 챌린지 토큰의 사용자 데이터를 클라이언트 슬롯에 복사하여 서버 애플리케이션에서 액세스할 수 있도록 합니다.
- 해당 클라이언트 슬롯에 대한 confirmed 플래그를 false로 설정합니다.
- 연결 유지 패킷으로 응답합니다.
  
### 연결된 클라이언트
클라이언트가 서버의 슬롯에 할당되면 논리적으로 연결됩니다.  
   
이 슬롯의 인덱스는 서버에서 클라이언트를 식별하는 데 사용되며 클라이언트 인덱스라고 합니다.  
  
서버가 해당 클라이언트의 주소와 포트에서 수신한 패킷은 해당 클라이언트 인덱스에 매핑되어 해당 클라이언트의 컨텍스트에서 처리됩니다.  
  
이러한 패킷에는 다음이 포함됩니다:  
- 연결 유지 패킷
- 연결 페이로드 패킷
- 연결 끊기 패킷  
    
서버는 연결된 클라이언트에서 받은 연결 페이로드 패킷을 버퍼링하여 페이로드 데이터를 넷코드 패킷으로 서버 애플리케이션에 전달할 수 있도록 합니다.  
  
서버 애플리케이션은 연결된 클라이언트에 연결 페이로드 패킷을 보낼 수도 있습니다.  
  
클라이언트로 전송된 연결 페이로드 패킷이 없는 경우 서버는 10HZ와 같은 일정한 속도로 연결 유지 패킷을 생성하여 해당 클라이언트로 전송합니다.  
  
클라이언트 슬롯에 대한 확정 플래그가 거짓인 경우, 해당 클라이언트로 전송되는 각 연결 페이로드 패킷에는 연결 유지 패킷이 앞에 전송됩니다. 이는 해당 클라이언트가 완전 연결 상태로 전환하는 데 필요한 클라이언트 인덱스와 최대 클라이언트 수를 해당 클라이언트에 전달합니다.  
  
서버가 미확인 클라이언트로부터 연결 페이로드 패킷 또는 연결 유지 패킷을 수신하면, 해당 클라이언트 슬롯에 대한 확정 플래그를 true로 설정하고 연결 페이로드 패킷 앞에 연결 유지 패킷 접두사를 더 이상 붙이지 않습니다.  
  
서버는 클라이언트의 연결을 끊으려는 경우 해당 클라이언트 슬롯을 재설정하기 전에 해당 클라이언트에 중복 연결 끊기 패킷을 여러 번 보냅니다.  
  
연결 토큰에 지정된 시간 초과 기간 내에 클라이언트로부터 연결 페이로드 패킷 또는 연결 유지 패킷을 받지 못하거나 서버가 클라이언트로부터 연결 끊기 패킷을 받으면 클라이언트 슬롯이 재설정되고 다른 클라이언트에서 연결할 수 있는 상태가 됩니다.     
  
    
<br>  
  	
 # 원 문서
**netcode** is a secure client/server protocol built on top of UDP.

It's intended for use by real-time multiplayer games, which need a connection oriented protocol but without the head of line blocking of TCP.

![connetion 2](https://github.com/user-attachments/assets/5c7e0c9b-17b6-4e84-a57b-13bdb55a9978)

netcode has the following features:

* Secure client connection with connect tokens. Only clients you authorize can connect to your server. This is _perfect_ for a game where you perform matchmaking in a web backend then send clients to a server.
* Client slot system. Servers have n slots for clients. Client are assigned to a slot when they connect to the server and are quickly denied connection if all slots are taken.
* Fast clean disconnect on client or server side of connection to open up the slot for a new client, plus timeouts for hard disconnects.
* Encrypted and signed packets. Packets cannot be tampered with or read by parties not involved in the connection. Cryptography is performed by the excellent [sodium library](https://libsodium.gitbook.io/doc).
* Many security features including robust protection against maliciously crafted packets, packet replay attacks and packet amplification attacks.
* Support for packet tagging which can significantly reduce jitter on Wi-Fi routers. Read [this article](https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/networking/overviews/qos-packet-tagging) for more details.

netcode is stable and production ready.

# Usage

Start by generating a random 32 byte private key. Do not share your private key with _anybody_. 

Especially, **do not include your private key in your client executable!**

Here is a test private key:

```c
static uint8_t private_key[NETCODE_KEY_BYTES] = { 0x60, 0x6a, 0xbe, 0x6e, 0xc9, 0x19, 0x10, 0xea, 
                                                  0x9a, 0x65, 0x62, 0xf6, 0x6f, 0x2b, 0x30, 0xe4, 
                                                  0x43, 0x71, 0xd6, 0x2c, 0xd1, 0x99, 0x27, 0x26,
                                                  0x6b, 0x3c, 0x60, 0xf4, 0xb7, 0x15, 0xab, 0xa1 };
```

Create a server with the private key:

```c
char * server_address = "127.0.0.1:40000";

struct netcode_server_config_t server_config;
netcode_default_server_config( &server_config );
memcpy( &server_config.private_key, private_key, NETCODE_KEY_BYTES );

struct netcode_server_t * server = netcode_server_create( server_address, &server_config, time );
if ( !server )
{
    printf( "error: failed to create server\n" );
    return 1;
}
```

Then start the server with the number of client slots you want:

```c
netcode_server_start( server, 16 );
```

To connect a client, your client should hit a REST API to your backend that returns a _connect token_.

Using a connect token secures your server so that only clients authorized with your backend can connect.

```c
netcode_client_connect( client, connect_token );
```

Once the client connects to the server, the client is assigned a client index and can exchange encrypted and signed packets with the server.

For more details please see [client.c](client.c) and [server.c](server.c)

# Source Code

This repository holds the implementation of netcode in C.

Other netcode implementations include:

* [netcode C# implementation](https://github.com/KillaMaaki/Netcode.IO.NET)
* [netcode Golang implementation](https://github.com/wirepair/netcode)
* [netcode Rust implementation](https://github.com/jaynus/netcode.io) (updated fork of [vvanders/netcode.io](https://github.com/vvanders/netcode.io))
* [netcode Rust implementation](https://github.com/benny-n/netcode) (new from scratch Rust implementation)
* [netcode for Unity](https://github.com/KillaMaaki/Unity-Netcode.IO)
* [netcode for UE4](https://github.com/RedpointGames/netcode.io-UE4)
* [netcode for Typescript](https://github.com/bennychen/netcode.io-typescript)

If you'd like to create your own implementation of netcode, please read the [netcode 1.02 standard](STANDARD.md).

# Contributors

These people are awesome:

* [Val Vanders](https://github.com/vvanders) - Rust Implementation
* [Walter Pearce](https://github.com/jaynus) - Rust Implementation
* [Isaac Dawson](https://github.com/wirepair) - Golang Implementation
* [Alan Stagner](https://github.com/KillaMaaki) - Unity integration, C# implementation
* [Jérôme Leclercq](https://github.com/SirLynix) - Support for random connect token nonce
* [Randy Gaul](https://github.com/RandyGaul) - Discovered vulnerability in replay protection
* [Benny Chen](https://github.com/bennychen) - Typescript Implementation
* [Benny Nazimov](https://github.com/benny-n) - Rust implementation

Thanks for your contributions to netcode!

# Author

The author of this library is [Glenn Fiedler](https://www.linkedin.com/in/glenn-fiedler-11b735302/).

Other open source libraries by the same author include: [reliable](https://github.com/mas-bandwidth/reliable), [serialize](https://github.com/mas-bandwidth/serialize), and [yojimbo](https://github.com/mas-bandwidth/yojimbo).

If you find this software useful, [please consider sponsoring it](https://github.com/sponsors/mas-bandwidth). Thanks!

# License

[BSD 3-Clause license](https://opensource.org/licenses/BSD-3-Clause).
