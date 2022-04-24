namespace Plus.Communication.ConnectionManager;

public interface ISocketManager
{
    /// <summary>
    ///     Occurs when a new connection was established
    /// </summary>
    event SocketManager.ConnectionEvent OnConnectionEvent;

    /// <summary>
    ///     Prepares the socket for connections
    /// </summary>
    public void Init(
        bool disableNagleAlgorithm,
        int maximumConnections,
        int maxIpConnectionCount,
        int listeningPort,
        IDataParser parser
    );

    /// <summary>
    ///     Destroys the current connection manager and disconnects all users
    /// </summary>
    void Destroy();

    /// <summary>
    ///     Reports a gameconnection as disconnected
    /// </summary>
    /// <param name="gameConnection">The connection which is logging out</param>
    void ReportDisconnect(ConnectionInformation gameConnection);
}