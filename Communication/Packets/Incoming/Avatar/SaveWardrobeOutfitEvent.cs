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
        var rawGender = packet.PopString();
        var gender = ClothingGenderExtensions.ParseFromString(rawGender);
        var processedLook = _figureDataManager.ValidateFigure(session.GetHabbo(), look, gender, session.GetHabbo().GetClothing().GetClothingParts);
        
        return _wardrobeManager.SaveWardrobe(session.GetHabbo(), slotId, processedLook, rawGender);
    }
}