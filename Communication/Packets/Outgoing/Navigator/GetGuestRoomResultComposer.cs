﻿using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Navigator;

internal class GetGuestRoomResultComposer : ServerPacket
{
    public GetGuestRoomResultComposer(GameClient session, RoomData data, bool isLoading, bool checkEntry)
        : base(ServerPacketHeader.GetGuestRoomResultMessageComposer)
    {
        WriteBoolean(isLoading);
        WriteInteger(data.Id);
        WriteString(data.Name);
        WriteInteger(data.OwnerId);
        WriteString(data.OwnerName);
        WriteInteger(RoomAccessUtility.GetRoomAccessPacketNum(data.Access));
        WriteInteger(data.UsersNow);
        WriteInteger(data.UsersMax);
        WriteString(data.Description);
        WriteInteger(data.TradeSettings);
        WriteInteger(data.Score);
        WriteInteger(0); //Top rated room rank.
        WriteInteger(data.Category);
        WriteInteger(data.Tags.Count);
        foreach (var tag in data.Tags) WriteString(tag);
        if (data.Group != null && data.Promotion != null)
        {
            WriteInteger(62);
            WriteInteger(data.Group?.Id ?? 0);
            WriteString(data.Group == null ? "" : data.Group.Name);
            WriteString(data.Group == null ? "" : data.Group.Badge);
            WriteString(data.Promotion != null ? data.Promotion.Name : "");
            WriteString(data.Promotion != null ? data.Promotion.Description : "");
            WriteInteger(data.Promotion?.MinutesLeft ?? 0);
        }
        else if (data.Group != null && data.Promotion == null)
        {
            WriteInteger(58);
            WriteInteger(data.Group?.Id ?? 0);
            WriteString(data.Group == null ? "" : data.Group.Name);
            WriteString(data.Group == null ? "" : data.Group.Badge);
        }
        else if (data.Group == null && data.Promotion != null)
        {
            WriteInteger(60);
            WriteString(data.Promotion != null ? data.Promotion.Name : "");
            WriteString(data.Promotion != null ? data.Promotion.Description : "");
            WriteInteger(data.Promotion?.MinutesLeft ?? 0);
        }
        else
            WriteInteger(56);
        WriteBoolean(checkEntry);
        WriteBoolean(false);
        WriteBoolean(false);
        WriteBoolean(false);
        WriteInteger(data.WhoCanMute);
        WriteInteger(data.WhoCanKick);
        WriteInteger(data.WhoCanBan);
        WriteBoolean(session.GetHabbo().GetPermissions().HasRight("mod_tool") || data.OwnerName == session.GetHabbo().Username);
        WriteInteger(data.ChatMode);
        WriteInteger(data.ChatSize);
        WriteInteger(data.ChatSpeed);
        WriteInteger(data.ExtraFlood);
        WriteInteger(data.ChatDistance);
    }
}