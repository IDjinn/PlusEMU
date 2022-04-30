using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Plus.HabboHotel.Avatar;

namespace Plus.Communication.Packets.Outgoing.Avatar;

internal class WardrobeComposer : ServerPacket
{
    public WardrobeComposer(IList<WardrobeSlot>? wardrobes) : base(ServerPacketHeader.WardrobeMessageComposer)
    {
        WriteInteger(1);
        if (wardrobes is null || !wardrobes.Any())
        {
            WriteInteger(0);
        }
        else
        {
            WriteInteger(wardrobes.Count);
            foreach (var slot in wardrobes)
            {
                WriteInteger(slot.SlotId);
                WriteString(slot.Look);
                WriteString(slot.Gender);
            }
        }
    }
}