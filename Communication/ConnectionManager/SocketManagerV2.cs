using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using NLog;

namespace Plus.Communication.ConnectionManager;


public class SocketManagerV2 :ISocketManagerV2
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ConcurrentDictionary<Guid, SocketConnection> _connections = new();
    private readonly Socket _listener;
    
    public string Ip { get; }
    public int Port { get; } = 1232;
    public bool IsListening { get; }
    

    public SocketManagerV2()
    {
        _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };
    }
    public void Init(int portId, int maxConnections, int connectionsPerIp, IDataParser parser, bool disableNaglesAlgorithm)
    {
        _listener.Bind(new IPEndPoint(IPAddress.Any, portId));
        _listener.Listen(100);
        _listener.BeginAccept(OnConnection, _listener);
        
        Logger.Info("Socket manager initialized and waiting for connections.");
    }

    private void OnConnection(IAsyncResult asyncResult)
    {
        var remoteSocket = (asyncResult.AsyncState as Socket)!.EndAccept(asyncResult);
        remoteSocket.NoDelay = true;
        var connection = new SocketConnection(remoteSocket);
        _connections.TryAdd(connection.Id, connection);
        
        connection.StartListen();
        Logger.Debug($"New connection from {connection.Ip.Address} with id {connection.Id}");
    }


    public void Destroy()
    {
        _listener.Close();
        _listener.Dispose();

        foreach (var connection in _connections.Values)
        {
            connection.Dispose();
        }
        
        _connections.Clear();
        Logger.Info("SocketManager has been disposed.");
    }
}