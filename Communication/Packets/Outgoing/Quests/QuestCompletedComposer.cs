﻿using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.Communication.Packets.Outgoing.Quests;

internal class QuestCompletedComposer : ServerPacket
{
    public QuestCompletedComposer(GameClient session, Quest quest)
        : base(ServerPacketHeader.QuestCompletedMessageComposer)
    {
        var amountInCat = PlusEnvironment.GetGame().GetQuestManager().GetAmountOfQuestsInCategory(quest.Category);
        var number = quest?.Number ?? amountInCat;
        var userProgress = quest == null ? 0 : session.GetHabbo().GetQuestProgress(quest.Id);
        WriteString(quest.Category);
        WriteInteger(number); // Quest progress in this cat
        WriteInteger(quest.Name.Contains("xmas2012") ? 1 : amountInCat); // Total quests in this cat
        WriteInteger(quest?.RewardType ?? 3); // Reward type (1 = Snowflakes, 2 = Love hearts, 3 = Pixels, 4 = Seashells, everything else is pixels
        WriteInteger(quest?.Id ?? 0); // Quest id
        WriteBoolean(quest == null ? false : session.GetHabbo().GetStats().QuestId == quest.Id); // Quest started
        WriteString(quest == null ? string.Empty : quest.ActionName);
        WriteString(quest == null ? string.Empty : quest.DataBit);
        WriteInteger(quest?.Reward ?? 0);
        WriteString(quest == null ? string.Empty : quest.Name);
        WriteInteger(userProgress); // Current progress
        WriteInteger(quest?.GoalData ?? 0); // Target progress
        WriteInteger(quest?.TimeUnlock ?? 0); // "Next quest available countdown" in seconds
        WriteString("");
        WriteString("");
        WriteBoolean(true); // ?
        WriteBoolean(true); // Activate next quest..
    }
}