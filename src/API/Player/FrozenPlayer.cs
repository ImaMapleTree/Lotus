using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.Extensions;
using Lotus.Managers.History.Events;
using Lotus.Roles;
using Lotus.Statuses;
using Lotus.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using static GameData;

namespace Lotus.API.Player;

public class FrozenPlayer
{
    public byte PlayerId;
    public string FriendCode;
    public string Name;
    public string ColorName;
    public CustomRole Role;
    public List<CustomRole> Subroles;
    public RemoteList<IStatus> Statuses = new();
    public uint Level;
    public PlayerOutfit Outfit;
    public ulong GameID;
    public IDeathEvent? CauseOfDeath;

    public PlayerControl MyPlayer => NullablePlayer == null ? NullablePlayer ??= GetPlayer() : NullablePlayer;

    public PlayerControl NullablePlayer;

    public FrozenPlayer(PlayerControl player)
    {
        Name = player.name;
        ColorName = player.Data.ColoredName();
        Level = player.Data.PlayerLevel;
        FriendCode = player.FriendCode;
        PlayerId = player.PlayerId;
        Outfit = player.CurrentOutfit;
        Role = player.GetCustomRole();
        Subroles = player.GetSubroles();
        GameID = player.GetGameID();

        Hooks.PlayerHooks.PlayerDeathHook.Bind($"{nameof(FrozenPlayer)}-{PlayerId}", pd =>
        {
            if (pd.Player == null || pd.Player.PlayerId != PlayerId) return;
            CauseOfDeath = pd.CauseOfDeath;
        }, true);
        this.NullablePlayer = player;
    }

    private PlayerControl GetPlayer()
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrOptional(p => p.FriendCode == FriendCode).OrElseGet(() => Utils.GetPlayerById(PlayerId)!);
    }

}