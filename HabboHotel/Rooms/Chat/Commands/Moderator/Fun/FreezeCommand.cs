﻿using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun;

internal class FreezeCommand : IChatCommand
{
    private readonly IGameClientManager _gameClientManager;
    public string Key => "freeze";
    public string PermissionRequired => "command_freeze";

    public string Parameters => "%username%";

    public string Description => "Prevent another user from walking.";

    public FreezeCommand(IGameClientManager gameClientManager)
    {
        _gameClientManager = gameClientManager;
    }

    public void Execute(GameClient session, Room room, string[] @params)
    {
        if (@params.Length == 1)
        {
            session.SendWhisper("Please enter the username of the user you wish to freeze.");
            return;
        }
        var targetClient = _gameClientManager.GetClientByUsername(@params[1]);
        if (targetClient == null)
        {
            session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
            return;
        }
        var targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(@params[1]);
        if (targetUser != null)
            targetUser.Frozen = true;
        session.SendWhisper("Successfully froze " + targetClient.GetHabbo().Username + "!");
    }
}