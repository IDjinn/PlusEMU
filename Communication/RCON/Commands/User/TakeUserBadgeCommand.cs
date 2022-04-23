﻿using System;

namespace Plus.Communication.Rcon.Commands.User;

internal class TakeUserBadgeCommand : IRconCommand
{
    public string Description => "This command is used to take a badge from a user.";

    public string Key => "take_user_badge";
    public string Parameters => "%userId% %badgeId%";

    public bool TryExecute(string[] parameters)
    {
        if (!int.TryParse(parameters[0], out var userId))
            return false;
        var client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
        if (client == null || client.GetHabbo() == null)
            return false;

        // Validate the badge
        if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
            return false;
        var badge = Convert.ToString(parameters[1]);
        if (client.GetHabbo().GetBadgeComponent().HasBadge(badge)) client.GetHabbo().GetBadgeComponent().RemoveBadge(badge);
        return true;
    }
}