using System.Threading.Tasks;
using Plus.Core.FigureData;
using Plus.Database;
using Plus.HabboHotel.GameClients;

using Dapper;
using Plus.HabboHotel.Avatar;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Avatar;

internal class SaveWardrobeOutfitEvent : IPacketEvent
{
    private readonly IFigureDataManager _figureDataManager;
    private readonly IWardrobeManager _wardrobeManager;

    public SaveWardrobeOutfitEvent(IFigureDataManager figureDataManager,IWardrobeManager wardrobeManager)
    {
        _figureDataManager = figureDataManager;
        _wardrobeManager = wardrobeManager;
    }

    public Task Parse(GameClient session, ClientPacket packet)
    {
        var slotId = packet.PopInt();
        var look = packet.PopString();
        var gender = packet.PopString();
        var processedLook = _figureDataManager.ProcessFigure(look, gender, session.GetHabbo().GetClothing().GetClothingParts, true);
        
        return _wardrobeManager.SaveWardrobe(session.GetHabbo(), slotId, processedLook, gender);
    }
}