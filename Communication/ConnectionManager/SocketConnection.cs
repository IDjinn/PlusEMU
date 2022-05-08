using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NLog;
using Plus.Communication.Encryption.Crypto.Prng;
using Plus.Communication.Packets.Incoming;

namespace Plus.Communication.ConnectionManager;


public class SocketConnection : ISocketConnection
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

    public Guid Id { get; } = Guid.NewGuid();
    public IPEndPoint Ip { get; }
    private readonly Socket _socket;
    private Arc4? _arc4;
    
    private bool _handshakeCompleted = false;

    public bool IsConnected => true;
    
    public SocketConnection(Socket socket)
    {
        _socket = socket;
        Ip = (socket.RemoteEndPoint as IPEndPoint)!;
    }

    public void StartListen() => ListenForPackets();
    private void ListenForPackets()
    {
        if(!IsConnected) return;
        
        var buffer= _bufferPool.Rent(ISocketManagerV2.BufferSize);
        try
        {
            _socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnReceive, buffer);
        }
        finally
        {
            _bufferPool.Return(buffer);
        }
    }

    private void OnReceive(IAsyncResult result)
    {
        try
        {
            var buffer = (result.AsyncState as byte[])!;
            var remainingLength = _socket.EndReceive(result);

            var isPolicyRequest = buffer[0] == 60;
            if (isPolicyRequest) // <policy-file-request/>\0
            {
                SendData(ISocketManagerV2.PolicyRequest);
            }
            else if(remainingLength >= 6)
            {
                var packets = new List<ClientPacket>();
                using var reader = new BinaryReader(new MemoryStream(buffer));
                do
                {
                    var packetLength = HabboEncryption.HabboEncoding.DecodeInt32(reader.ReadBytes(4));
                    var packetHeader = HabboEncryption.HabboEncoding.DecodeInt16(reader.ReadBytes(2));
                    var packet = new ClientPacket(packetHeader, reader.ReadBytes(packetLength - 2));
                    packets.Add(packet);
                    remainingLength -= packetLength;
                } while (remainingLength > 0);

                if (packets.Any()) OnPacketsReceivedEvent?.BeginInvoke(this, packets, null, this);
            }
        }
        catch (SocketException socketException)
        {
            // TODO: handle socket exception
        }
        catch (ObjectDisposedException objectDisposedException)
        {
            //TODO: handle object disposed exception
        }
        finally
        {
            ListenForPackets();
        }
    }

    public void Disconnect() => _socket.Disconnect(false);

    public void SendData(byte[] data) => _socket.Send(data);
    public void SendData(byte[] data, int offset, int size) => _socket.Send(data, offset, size, SocketFlags.None);
    
    public event ISocketConnection.OnPacketsReceived OnPacketsReceivedEvent;


    public void Dispose()
    {
        Disconnect();
        _socket.Dispose();
        //_arc4?.Dispose();
    }
}