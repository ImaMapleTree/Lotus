using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.Extensions;
using Lotus.Roles;
using Lotus.Utilities;
using VentLib.Utilities.Extensions;
using static GameData;

namespace Lotus.API.Player;

public class FrozenPlayer
{
    public byte PlayerId;
    public string FriendCode;
    public string Name;
    public CustomRole Role;
    public List<CustomRole> Subroles;
    public uint Level;
    public PlayerOutfit Outfit;
    public ulong GameID;
    public string? CauseOfDeath;

    public PlayerControl MyPlayer => NullablePlayer == null ? NullablePlayer ??= GetPlayer() : NullablePlayer;

    public PlayerControl NullablePlayer;

    public FrozenPlayer(PlayerControl player)
    {
        Name = player.name;
        Level = player.Data.PlayerLevel;
        FriendCode = player.FriendCode;
        PlayerId = player.PlayerId;
        Outfit = player.CurrentOutfit;
        Role = player.GetCustomRole();
        Subroles = player.GetSubroles();
        GameID = player.GetGameID();
        
        Hooks.PlayerHooks.PlayerDeathHook.Bind($"{nameof(FrozenPlayer)}-{PlayerId}", pd => CauseOfDeath = pd.CauseOfDeath, true);
        this.NullablePlayer = player;
    }

    private PlayerControl GetPlayer()
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrOptional(p => p.FriendCode == FriendCode).OrElseGet(() => Utils.GetPlayerById(PlayerId)!);
    }

}