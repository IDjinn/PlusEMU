﻿using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Moderation;

internal class ModeratorRoomInfoComposer : ServerPacket
{
    public ModeratorRoomInfoComposer(RoomData data, bool ownerInRoom)
        : base(ServerPacketHeader.ModeratorRoomInfoMessageComposer)
    {
        WriteInteger(data.Id);
        WriteInteger(data.UsersNow);
        WriteBoolean(ownerInRoom); // owner in room
        WriteInteger(data.OwnerId);
        WriteString(data.OwnerName);
        WriteBoolean(data != null);
        WriteString(data.Name);
        WriteString(data.Description);
        WriteInteger(data.Tags.Count);
        foreach (var tag in data.Tags) WriteString(tag);
        WriteBoolean(false);
    }
}