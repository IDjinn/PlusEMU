using System;
using System.Collections.Generic;
using System.Net;
using Plus.Communication.Packets.Incoming;

namespace Plus.Communication.ConnectionManager;


public interface ISocketConnection: IDisposable
{
    public Guid Id { get; }
    public IPEndPoint Ip { get; }
    public bool IsConnected { get; }
    
    void StartListen();
    public void Disconnect();
    public void SendData(byte[] data);
    
    public delegate void OnPacketsReceived(SocketConnection connection, IEnumerable<ClientPacket> packet);
    public event OnPacketsReceived OnPacketsReceivedEvent;
}