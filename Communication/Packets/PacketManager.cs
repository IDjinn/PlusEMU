using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Plus.Communication.Attributes;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets;

public sealed class PacketManager : IPacketManager
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.Communication.Packets");

    private readonly Dictionary<int, Type> _headerToPacketMapping = new();
    private readonly Dictionary<int, IPacketEvent> _incomingPackets = new();
    private readonly HashSet<int> _handshakePackets = new();
    private readonly Dictionary<int, string> _packetNames = new();

    public PacketManager(IEnumerable<IPacketEvent> incomingPackets)
    {
        foreach (var packet in incomingPackets)
        {
            var field = typeof(ClientPacketHeader).GetField(packet.GetType().Name, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (field == null)
            {
                Log.Warn("No incoming header defined for {packet}", packet.GetType().Name);
                continue;
            }
            var header = (int) field.GetValue(null);
            _incomingPackets.Add(header, packet);
            _packetNames.Add(header, packet.GetType().Name);
            if (packet.GetType().GetCustomAttribute<NoAuthenticationRequiredAttribute>() != null)
            {
                _handshakePackets.Add(header);
            }
        }
    }

    public void ExecutePacket(GameClient session, ClientPacket packet)
    {
        if (!_incomingPackets.TryGetValue(packet.Id, out var packetEvent))
        {
            Log.Debug("Unhandled Packet [{PacketId}] {Packet}", packet.Id, packet);
            return;
        }

        if (Debugger.IsAttached)
        {
            Log.Debug("Handled Packet: [{PacketId}] {PacketName}", packet.Id,
                _packetNames.ContainsKey(packet.Id) ? _packetNames[packet.Id] : "UnnamedPacketEvent"
            );
        }

        if (!_handshakePackets.Contains(packet.Id) && session.GetHabbo() == null)
        {
            Log.Debug($"Session {session.ConnectionId} tried execute packet {packet.Id} but didn't handshake yet.");
            return;
        }

        try
        {
            packetEvent.Parse(session, packet);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while parsing packet {packet}", packet.Id);
        }
    }

    public void UnregisterAll()
    {
        _headerToPacketMapping.Clear();
    }
}