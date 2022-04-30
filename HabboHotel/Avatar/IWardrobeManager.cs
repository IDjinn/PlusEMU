using System.Collections.Generic;
using System.Threading.Tasks;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Avatar
{
    public interface IWardrobeManager
    {
        Task<IList<WardrobeSlot>?> GetWardrobe(Habbo habbo);
        Task SaveWardrobe(Habbo habbo, int slotId, string look, string gender);
    }
}