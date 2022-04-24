using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets;

public interface IPacketManager
{
    void ExecutePacket(GameClient session, ClientPacket packet);
    void UnregisterAll();
}