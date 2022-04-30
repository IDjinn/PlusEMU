using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Plus.Database;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Avatar;

public class WardrobeManager : IWardrobeManager
{
    private readonly IDatabase _database;
    public WardrobeManager(IDatabase database)
    {
        _database = database;
    }

    public async Task<IList<WardrobeSlot>?> GetWardrobe(Habbo habbo)
    {
        using var connection =  _database.Connection();
        var wardrobe = await connection.QueryAsync<WardrobeSlot>("SELECT * FROM `user_wardrobe` WHERE user_id = @user_id", new { user_id = habbo.Id });
        return wardrobe?.ToList();
    }

    public async Task SaveWardrobe(Habbo habbo, int slotId, string look, string gender)
    {
        using var connection = _database.Connection();
        await connection.ExecuteAsync(
            "REPLACE INTO `user_wardrobe` (user_id, slot_id, look, gender) VALUE (@user_id, @slot_id, @look, @gender)",
            new {user_id = habbo.Id, slot_id = slotId,look, gender}
        );
    }
}    
    
