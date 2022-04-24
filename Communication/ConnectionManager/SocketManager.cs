using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;
using Plus.Communication.ConnectionManager.Socket_Exceptions;

namespace Plus.Communication.ConnectionManager;

public class SocketManager : ISocketManager
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.Communication.ConnectionManager");
    public delegate void ConnectionEvent(ConnectionInformation connection);

    private bool _acceptConnections;
    private int _connectionIdCounter;

    private Socket _connectionListener;
    private bool _disableNagleAlgorithm;
    private ConcurrentDictionary<string, int> _ipConnectionsCount = new();
    private int _maximumConnections;
    private int _maxIpConnectionCount;

    private IDataParser _parser;
    private int _listeningPort;

    public event ConnectionEvent OnConnectionEvent;
    

    /// <summary>
    ///     Prepares the socket for connections
    /// </summary>
    public void Init(
        bool disableNagleAlgorithm, 
        int maximumConnections, 
        int maxIpConnectionCount,
        int listeningPort,
        IDataParser parser
        )
    {
        _disableNagleAlgorithm = disableNagleAlgorithm;
        _maximumConnections = maximumConnections;
        _maxIpConnectionCount = maxIpConnectionCount;
        _parser = parser;
        _listeningPort = listeningPort;
        
        _connectionListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = _disableNagleAlgorithm
        };
        try
        {
            _connectionListener.Bind(new IPEndPoint(IPAddress.Any, _listeningPort));
            _connectionListener.Listen(100);
            _acceptConnections = true;
            _connectionListener.BeginAccept(NewConnectionRequest, _connectionListener);
            Log.Info("Successfully setup GameSocketManager on port (" + _listeningPort + ")!");
            Log.Info("Maximum connections per IP has been set to [" + _maximumConnections + "]!");
        }
        catch (SocketException ex)
        {
            Log.Error(ex, "Failed to setup GameSocketManager on port ({Port}): {Message}", _listeningPort, ex.Message);
            Environment.Exit(0); // exit or perform shutdown?
        }
    }

    public void Destroy()
    {
        _acceptConnections = false;
        try
        {
            _connectionListener.Close();
        }
        catch
        {
            //ignored
        }
        _connectionListener = null;
    }

    private void NewConnectionRequest(IAsyncResult iAr)
    {
        if (_connectionListener != null)
        {
            if (_acceptConnections)
            {
                try
                {
                    var replyFromComputer = ((Socket)iAr.AsyncState).EndAccept(iAr);
                    replyFromComputer.NoDelay = _disableNagleAlgorithm;
                    var ip = replyFromComputer.RemoteEndPoint.ToString().Split(':')[0];
                    var connectionCount = GetAmountOfConnectionFromIp(ip);
                    if (connectionCount < _maxIpConnectionCount)
                    {
                        Interlocked.Increment(ref _connectionIdCounter);
                        var c = new ConnectionInformation(_connectionIdCounter, replyFromComputer, _parser.Clone() as IDataParser, ip);
                        ReportUserLogin(ip);
                        c.ConnectionChanged += OnConnectionChanged;
                        if (OnConnectionEvent != null)
                            OnConnectionEvent(c);
                    }
                    else
                        Log.Info("Connection denied from [" + replyFromComputer.RemoteEndPoint.ToString().Split(':')[0] + "]. Too many connections (" + connectionCount + ").");
                }
                catch { }
                finally
                {
                    _connectionListener.BeginAccept(NewConnectionRequest, _connectionListener);
                }
            }
        }
    }

    private void OnConnectionChanged(ConnectionInformation information, ConnectionState state)
    {
        if (state == ConnectionState.Closed) ReportDisconnect(information);
    }

    public void ReportDisconnect(ConnectionInformation gameConnection)
    {
        gameConnection.ConnectionChanged -= OnConnectionChanged;
        ReportUserLogout(gameConnection.GetIp());
        //activeConnections.Remove(gameConnection.getConnectionID());
    }

    private void ReportUserLogin(string ip)
    {
        AlterIpConnectionCount(ip, GetAmountOfConnectionFromIp(ip) + 1);
    }

    private void ReportUserLogout(string ip)
    {
        AlterIpConnectionCount(ip, GetAmountOfConnectionFromIp(ip) - 1);
    }

    private void AlterIpConnectionCount(string ip, int amount)
    {
        if (_ipConnectionsCount.ContainsKey(ip)) _ipConnectionsCount.TryRemove(ip, out var _);
        _ipConnectionsCount.TryAdd(ip, amount);
    }

    private int GetAmountOfConnectionFromIp(string ip)
    {
        if (_ipConnectionsCount.ContainsKey(ip)) return _ipConnectionsCount[ip];
        return 0;
    }
}