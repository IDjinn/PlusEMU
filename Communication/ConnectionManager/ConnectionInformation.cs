using System;
using System.Net.Sockets;

namespace Plus.Communication.ConnectionManager;

public class ConnectionInformation : IConnectionInformation
{
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
        if (ConnectionChanged != null)
            ConnectionChanged.Invoke(this, ConnectionState.Open);
    }

    public IDataParser Parser { get; set; }

    public void Dispose()
    {
        if (_isConnected) Disconnect();
        GC.SuppressFinalize(this);
    }

    public event ConnectionChange ConnectionChanged;

    public void StartPacketProcessing()
    {
        if (!_isConnected)
        {
            _isConnected = true;
            //Out.writeLine("Starting packet processsing of client [" + this.connectionID + "]", Out.logFlags.lowLogLevel);
            try
            {
                _dataSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, IncomingDataPacket, _dataSocket);
            }
            catch
            {
                Disconnect();
            }
        }
    }

    public string GetIp() => _ip;

    public int GetConnectionId() => _connectionId;

    public void Disconnect()
    {
        try
        {
            if (_isConnected)
            {
                _isConnected = false;

                //Out.writeLine("Connection [" + this.connectionID + "] has been disconnected", Out.logFlags.BelowStandardlogLevel);
                try
                {
                    if (_dataSocket != null && _dataSocket.Connected)
                    {
                        _dataSocket.Shutdown(SocketShutdown.Both);
                        _dataSocket.Close();
                    }
                }
                catch
                {
                    //ignored
                }
                _dataSocket.Dispose();
                Parser.Dispose();
                try
                {
                    if (ConnectionChanged != null)
                        ConnectionChanged.Invoke(this, ConnectionState.Closed);
                }
                catch
                {
                    //ignored
                }
                ConnectionChanged = null;
            }
        }
        catch { }
    }

    private void IncomingDataPacket(IAsyncResult iAr)
    {
        //Out.writeLine("Packet received from client [" + this.connectionID + "]", Out.logFlags.lowLogLevel);
        int bytesReceived;
        try
        {
            //The amount of bytes received in the packet
            bytesReceived = _dataSocket.EndReceive(iAr);
        }
        catch //(Exception e)
        {
            Disconnect();
            return;
        }
        if (bytesReceived == 0)
        {
            Disconnect();
            return;
        }

        try
        {
            var packet = new byte[bytesReceived];
            Array.Copy(_buffer, packet, bytesReceived);
            HandlePacketData(packet);
        }
        catch //(Exception e)
        {
            Disconnect();
        }
        finally
        {
            try
            {
                //and we keep looking for the next packet
                _dataSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, IncomingDataPacket, _dataSocket);
            }
            catch //(Exception e)
            {
                Disconnect();
            }
        }
    }

    private void HandlePacketData(byte[] packet)
    {
        if (Parser != null) Parser.HandlePacketData(packet);
    }

    public void SendData(byte[] packet)
    {
        try
        {
            if (!_isConnected)
                return;

            //Console.WriteLine(string.Format("Data from server => [{0}]", packetData));
            _dataSocket.BeginSend(packet, 0, packet.Length, 0, _sendCallback, null);
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
}