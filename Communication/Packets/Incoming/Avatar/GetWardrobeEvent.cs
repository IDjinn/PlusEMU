using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Avatar;
using Plus.HabboHotel.Avatar;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Avatar;

internal class GetWardrobeEvent : IPacketEvent
{
    private readonly IWardrobeManager _wardrobeManager;
    public GetWardrobeEvent(IWardrobeManager wardrobeManager)
    {
        _wardrobeManager = wardrobeManager;
    }
    
    public async Task Parse(GameClient session, ClientPacket packet)
    {
        var wardrobe = await _wardrobeManager.GetWardrobe(session.GetHabbo());
        session.SendPacket(new WardrobeComposer(wardrobe));
    }
}