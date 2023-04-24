using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Managers.History;

public class PlayerHistory
{
    public byte PlayerId;
    public UniquePlayerId UniquePlayerId;
    public string RealName;
    public CustomRole Role;
    public List<CustomRole> Subroles;
    public PlayerStatus Status;

    public PlayerHistory(PlayerControl source)
    {
        PlayerId = source.PlayerId;
        RealName = source.UnalteredName();
        Role = source.GetCustomRole();
        Subroles = source.GetSubroles();
        UniquePlayerId = source.UniquePlayerId();
        if (source.IsAlive()) Status = PlayerStatus.Alive;
        else if (source.Data.Disconnected) Status = PlayerStatus.Disconnected;
        else Status = Game.GameHistory.Events
                .FirstOrOptional(ev => ev is ExiledEvent exiledEvent && exiledEvent.Player().PlayerId == source.PlayerId)
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