using System;
using System.Net.Sockets;
using NLog;

namespace Plus.Communication.ConnectionManager;

public class ConnectionInformation : IConnectionInformation
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.Communication.ConnectionInformation");
    public delegate void ConnectionChange(ConnectionInformation information, ConnectionState state);

    private readonly byte[] _buffer;
    private readonly int _connectionId;
    private readonly Socket _dataSocket;
    private readonly string _ip;
    private readonly AsyncCallback _sendCallback;
    private bool _isConnected;

    /// <summary>
    ///     Creates a new Connection witht he given information
    /// </summary>
    /// <param name="dataStream">The Socket of the connection</param>
    /// <param name="connectionId">The id of the connection</param>
    /// <param name="parser">The data parser for the connection</param>
    /// <param name="ip">The IP Address for the connection</param>
    public ConnectionInformation(int connectionId, Socket dataStream, IDataParser parser, string ip)
    {
        Parser = parser;
        _buffer = new byte[GameSocketManagerStatics.BufferSize];
        _dataSocket = dataStream;
        _dataSocket.SendBufferSize = GameSocketManagerStatics.BufferSize;
        _ip = ip;
        _sendCallback = SentData;
        _connectionId = connectionId;
    }

    public IDataParser Parser { get; set; }

    public void Dispose()
    {
        Disconnect();
        GC.SuppressFinalize(this);
    }

    public event ConnectionChange ConnectionChanged;

    public void StartPacketProcessing()
    {
        ConnectionChanged?.Invoke(this, ConnectionState.Open);
        _isConnected = true;
        
        Log.Debug($"Starting packet processing of client [{_connectionId}]");
        try
        {
            _dataSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, IncomingDataPacket, _dataSocket);
        }
        catch
        {
            Disconnect();
        }
    }

    public string GetIp() => _ip;

    public int GetConnectionId() => _connectionId;

    public void Disconnect()
    {
        try
        {
            _isConnected = false;
            if (_dataSocket.Connected)
            {
                _dataSocket.Shutdown(SocketShutdown.Both);
                _dataSocket.Close();
            }

            _dataSocket.Dispose();
            Parser.Dispose();
            ConnectionChanged?.Invoke(this, ConnectionState.Closed);
            ConnectionChanged = null;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while trying disconnect session id [{ConnectionId}]: {Message}",_connectionId, e.Message);
        }
    }

    private void IncomingDataPacket(IAsyncResult iAr)
    {
        try
        {
            var bytesReceived = _dataSocket.EndReceive(iAr);
            if (bytesReceived == 0)
            {
                Disconnect();
                return;
            }

            var packet = new byte[bytesReceived];
            Array.Copy(_buffer, packet, bytesReceived);
            Parser.HandlePacketData(packet);
        }
        catch (Exception e)
        {
            Log.Error(e,"Error while receiving packet from client id [{ConnectionId}]: {Message}", _connectionId, e.Message);
            Disconnect();
        }
        finally
        {
            _dataSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, IncomingDataPacket, _dataSocket);
        }
    }

    public void SendData(byte[] packet)
    {
        try
        {
            if (!_isConnected)
                return;

            _dataSocket.BeginSend(packet, 0, packet.Length, 0, SendDataCallback, null);
        }
        catch
        {
            Disconnect();
        }
    }

    private void SentData(IAsyncResult iAr)
    {
        try
        {
            _dataSocket.EndSend(iAr);
        }
        catch
        {
            Disconnect();
        }
    }
    
    private void SendDataCallback(IAsyncResult callback)
    {
        
    }
}