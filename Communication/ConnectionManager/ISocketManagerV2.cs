namespace Plus.Communication.ConnectionManager;


public interface ISocketManagerV2
{
    public static readonly byte[] PolicyRequest = PlusEnvironment.GetDefaultEncoding().GetBytes("<?xml version=\"1.0\"?>\r\n" +
        "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n" +
        "<cross-domain-policy>\r\n" +
        "<allow-access-from domain=\"*\" to-ports=\"1-31111\" />\r\n" +
        "</cross-domain-policy>\x0");
    
    public const int BufferSize = 5120 * 10;
    public const int MaxPacketSize = 5120;
    public string Ip { get; }
    public int Port { get; }
    public bool IsListening { get; }
    
    
    /// <summary>
    ///     Initializes the connection instance
    /// </summary>
    /// <param name="portId">The ID of the port this item should listen on</param>
    /// <param name="maxConnections">The maximum amount of connections</param>
    /// <param name="connectionsPerIp">The maximum allowed connections per IP Address</param>
    /// <param name="parser">The data parser for the connection</param>
    /// <param name="disableNaglesAlgorithm">Disable nagles algorithm</param>
    void Init(int portId, int maxConnections, int connectionsPerIp, IDataParser parser, bool disableNaglesAlgorithm);
    public void Destroy();
    
}