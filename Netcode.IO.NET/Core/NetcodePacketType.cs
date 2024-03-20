
namespace NetcodeIO.NET;

/// <summary>
	/// Packet type code
	/// </summary>
	internal enum NetcodePacketType
{
    /// <summary>
    /// Connection request (sent from client to server)
    /// </summary>
    ConnectionRequest = 0,

    /// <summary>
    /// Connection denied (sent from server to client)
    /// </summary>
    ConnectionDenied = 1,

    /// <summary>
    /// Connection challenge (sent from server to client)
    /// </summary>
    ConnectionChallenge = 2,

    /// <summary>
    /// Challenge response (sent from client to server)
    /// </summary>
    ChallengeResponse = 3,

    /// <summary>
    /// Connection keep-alive (sent by both client and server)
    /// </summary>
    ConnectionKeepAlive = 4,

    /// <summary>
    /// Connection payload (sent by both client and server)
    /// </summary>
    ConnectionPayload = 5,

    /// <summary>
    /// Connection disconnect (sent by both client and server)
    /// </summary>
    ConnectionDisconnect = 6,

    /// <summary>
    /// Invalid packet
    /// </summary>
    InvalidPacket = 7,
}
