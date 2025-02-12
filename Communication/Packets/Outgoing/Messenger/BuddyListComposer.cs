﻿using System;
using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Outgoing.Messenger;

internal class BuddyListComposer : ServerPacket
{
    public BuddyListComposer(ICollection<MessengerBuddy> friends, Habbo player, int pages, int page)
        : base(ServerPacketHeader.BuddyListMessageComposer)
    {
        WriteInteger(pages); // Pages
        WriteInteger(page); // Page
        WriteInteger(friends.Count);
        foreach (var friend in friends.ToList())
        {
            var relationship = player.Relationships.FirstOrDefault(x => x.Value.UserId == Convert.ToInt32(friend.Id)).Value;
            WriteInteger(friend.Id);
            WriteString(friend.Username);
            WriteInteger(1); //Gender.
            WriteBoolean(friend.IsOnline);
            WriteBoolean(friend.IsOnline && friend.InRoom);
            WriteString(friend.IsOnline ? friend.Look : string.Empty);
            WriteInteger(0); // category id
            WriteString(friend.IsOnline ? friend.Motto : string.Empty);
            WriteString(string.Empty); //Alternative name?
            WriteString(string.Empty);
            WriteBoolean(true);
            WriteBoolean(false);
            WriteBoolean(false); //Pocket Habbo user.
            WriteShort(relationship?.Type ?? 0);
        }
    }
}