﻿using Plus.HabboHotel.Users.Effects;

namespace Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;

internal class AvatarEffectActivatedComposer : ServerPacket
{
    public AvatarEffectActivatedComposer(AvatarEffect effect)
        : base(ServerPacketHeader.AvatarEffectActivatedMessageComposer)
    {
        WriteInteger(effect.SpriteId);
        WriteInteger((int)effect.Duration);
        WriteBoolean(false); //Permanent
    }
}