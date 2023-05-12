using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Managers.History.Events;
using TOHTOR.Player;
using TOHTOR.Roles;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Managers.History;

public class PlayerHistory
{
    public byte PlayerId;
    public UniquePlayerId UniquePlayerId;
    public string Name;
    public CustomRole Role;
    public List<CustomRole> Subroles;
    public PlayerStatus Status;
    public uint Level;
    public GameData.PlayerOutfit Outfit;
    public ulong GameID;

    public PlayerHistory(FrozenPlayer frozenPlayer)
    {
        PlayerId = frozenPlayer.PlayerId;
        Name = frozenPlayer.Name;
        Role = frozenPlayer.Role;
        Subroles = frozenPlayer.Subroles;
        UniquePlayerId = UniquePlayerId.FromFriendCode(frozenPlayer.FriendCode);
        Level = frozenPlayer.Level;
        Outfit = frozenPlayer.Outfit;
        GameID = frozenPlayer.GameID;
        if (frozenPlayer.NullablePlayer != null && frozenPlayer.NullablePlayer.IsAlive()) Status = PlayerStatus.Alive;
        else if (frozenPlayer.NullablePlayer == null || frozenPlayer.NullablePlayer.Data.Disconnected) Status = PlayerStatus.Disconnected;
        else Status = Game.MatchData.GameHistory.Events
                .FirstOrOptional(ev => ev is ExiledEvent exiledEvent && exiledEvent.Player().PlayerId == PlayerId)
                .Map(_ => PlayerStatus.Exiled)
                .OrElse(PlayerStatus.Dead);
    }
}

public enum PlayerStatus
{
    Alive,
    Exiled,
    Dead,
    Disconnected
}