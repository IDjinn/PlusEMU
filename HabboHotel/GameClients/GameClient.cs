using System;
using Plus.Communication;
using Plus.Communication.ConnectionManager;
using Plus.Communication.Encryption.Crypto.Prng;
using Plus.Communication.Interfaces;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.BuildersClub;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Inventory.Achievements;
using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Sound;
using Plus.Core;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger.FriendBar;
using Plus.HabboHotel.Users.UserData;

namespace Plus.HabboHotel.GameClients;

public class GameClient
{
    private ConnectionInformation _connection;
    private bool _disconnected;
    private Habbo? _habbo;
    private GamePacketParser _packetParser;
    public string MachineId = string.Empty;
    public Arc4? Rc4Client;

    public GameClient(int clientId, ConnectionInformation connection)
    {
        ConnectionId = clientId;
        _connection = connection;
        _packetParser = new GamePacketParser(this);
        PingCount = 0;
    }

    public int PingCount { get; set; }

    public int ConnectionId { get; }

    private void SwitchParserRequest()
    {
        _packetParser.SetConnection(_connection);
        _packetParser.OnNewPacket += ParserOnNewPacket;
        var data = (_connection.Parser as InitialPacketParser).CurrentData;
        _connection.Parser.Dispose();
        _connection.Parser = _packetParser;
        _connection.Parser.HandlePacketData(data);
    }

    private void ParserOnNewPacket(ClientPacket message) =>
        PlusEnvironment.GetGame().GetPacketManager().ExecutePacket(this, message);

    private void PolicyRequest()
    {
        _connection.SendData(PlusEnvironment.GetDefaultEncoding().GetBytes("<?xml version=\"1.0\"?>\r\n" +
                                                                           "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n" +
                                                                           "<cross-domain-policy>\r\n" +
                                                                           "<allow-access-from domain=\"*\" to-ports=\"1-31111\" />\r\n" +
                                                                           "</cross-domain-policy>\x0"));
    }


    public void StartConnection()
    {
        PingCount = 0;
        (_connection.Parser as InitialPacketParser).PolicyRequest += PolicyRequest;
        (_connection.Parser as InitialPacketParser).SwitchParserRequest += SwitchParserRequest;
        _connection.StartPacketProcessing();
    }

    public bool TryAuthenticate(string authTicket)
    {
        try
        {
            var userData = UserDataFactory.GetUserData(authTicket, out var errorCode);
            if (errorCode == 1 || errorCode == 2)
            {
                Disconnect();
                return false;
            }

            //Let's have a quick search for a ban before we successfully authenticate..
            if (!string.IsNullOrEmpty(MachineId))
            {
                if (PlusEnvironment.GetGame().GetModerationManager().IsBanned(MachineId, out _))
                {
                    if (PlusEnvironment.GetGame().GetModerationManager().MachineBanCheck(MachineId))
                    {
                        Disconnect();
                        return false;
                    }
                }
            }
            if (userData.User != null)
            {
                if (PlusEnvironment.GetGame().GetModerationManager().IsBanned(userData.User.Username, out _))
                {
                    if (PlusEnvironment.GetGame().GetModerationManager().UsernameBanCheck(userData.User.Username))
                    {
                        Disconnect();
                        return false;
                    }
                }
            }
            if (userData.User == null) //Possible NPE
                return false;
            PlusEnvironment.GetGame().GetClientManager().RegisterClient(this, userData.UserId, userData.User.Username);
            _habbo = userData.User;
            if (_habbo != null)
            {
                userData.User.Init(this, userData);
                SendPacket(new AuthenticationOkComposer());
                SendPacket(new AvatarEffectsComposer(_habbo.Effects().GetAllEffects));
                SendPacket(new NavigatorSettingsComposer(_habbo.HomeRoom));
                SendPacket(new FavouritesComposer(userData.User.FavoriteRooms));
                SendPacket(new FigureSetIdsComposer(_habbo.GetClothing().GetClothingParts));
                SendPacket(new UserRightsComposer(_habbo.Rank));
                SendPacket(new AvailabilityStatusComposer());
                SendPacket(new AchievementScoreComposer(_habbo.GetStats().AchievementPoints));
                SendPacket(new BuildersClubMembershipComposer());
                SendPacket(new CfhTopicsInitComposer(PlusEnvironment.GetGame().GetModerationManager().UserActionPresets));
                SendPacket(new BadgeDefinitionsComposer(PlusEnvironment.GetGame().GetAchievementManager().Achievements));
                SendPacket(new SoundSettingsComposer(_habbo.ClientVolume, _habbo.ChatPreference, _habbo.AllowMessengerInvites, _habbo.FocusPreference,
                    FriendBarStateUtility.GetInt(_habbo.FriendbarState)));
                //SendMessage(new TalentTrackLevelComposer());
                if (GetHabbo().GetMessenger() != null)
                    GetHabbo().GetMessenger().OnStatusChanged(true);
                if (!string.IsNullOrEmpty(MachineId))
                {
                    if (_habbo.MachineId != MachineId)
                    {
                        using var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
                        dbClient.SetQuery("UPDATE `users` SET `machine_id` = @MachineId WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("MachineId", MachineId);
                        dbClient.AddParameter("id", _habbo.Id);
                        dbClient.RunQuery();
                    }
                    _habbo.MachineId = MachineId;
                }
                if (PlusEnvironment.GetGame().GetPermissionManager().TryGetGroup(_habbo.Rank, out var group))
                {
                    if (!string.IsNullOrEmpty(group.Badge))
                    {
                        if (!_habbo.GetBadgeComponent().HasBadge(group.Badge))
                            _habbo.GetBadgeComponent().GiveBadge(group.Badge, true, this);
                    }
                }
                if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(_habbo.VipRank, out var subData))
                {
                    if (!string.IsNullOrEmpty(subData.Badge))
                    {
                        if (!_habbo.GetBadgeComponent().HasBadge(subData.Badge))
                            _habbo.GetBadgeComponent().GiveBadge(subData.Badge, true, this);
                    }
                }
                if (!PlusEnvironment.GetGame().GetCacheManager().ContainsUser(_habbo.Id))
                    PlusEnvironment.GetGame().GetCacheManager().GenerateUser(_habbo.Id);
                _habbo.Look = PlusEnvironment.GetFigureManager().ProcessFigure(_habbo.Look, _habbo.Gender, _habbo.GetClothing().GetClothingParts, true);
                _habbo.InitProcess();
                if (userData.User.GetPermissions().HasRight("mod_tickets"))
                {
                    SendPacket(new ModeratorInitComposer(
                        PlusEnvironment.GetGame().GetModerationManager().UserMessagePresets,
                        PlusEnvironment.GetGame().GetModerationManager().RoomMessagePresets,
                        PlusEnvironment.GetGame().GetModerationManager().GetTickets));
                }
                if (PlusEnvironment.GetSettingsManager().TryGetValue("user.login.message.enabled") == "1")
                    SendPacket(new MotdNotificationComposer(PlusEnvironment.GetLanguageManager().TryGetValue("user.login.message")));
                PlusEnvironment.GetGame().GetRewardManager().CheckRewards(this);
                return true;
            }
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
        }
        return false;
    }

    public void SendWhisper(string message, int colour = 0)
    {
        if (GetHabbo() == null || GetHabbo().CurrentRoom == null)
            return;
        var user = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().Username);
        if (user == null)
            return;
        SendPacket(new WhisperComposer(user.VirtualId, message, 0, colour == 0 ? user.LastBubble : colour));
    }

    public void SendNotification(string message) => SendPacket(new BroadcastMessageAlertComposer(message));

    public void SendPacket(IServerPacket message) => GetConnection().SendData(message.GetBytes());

    public ConnectionInformation GetConnection() => _connection;

    public Habbo GetHabbo() => _habbo!;

    public void Disconnect()
    {
        try
        {
            if (GetHabbo() != null)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery(GetHabbo().GetQueryString);
                }
                GetHabbo().OnDisconnect();
            }
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException(e);
        }
        if (!_disconnected)
        {
            if (_connection != null)
                _connection.Dispose();
            _disconnected = true;
        }
    }

    public void Dispose()
    {
        if (GetHabbo() != null)
            GetHabbo().OnDisconnect();
        MachineId = string.Empty;
        _disconnected = true;
        _habbo = null;
        _connection = null;
        Rc4Client = null;
        _packetParser = null;
    }
}