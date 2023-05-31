using Lotus.Factions;
using Lotus.Options;
using Lotus.Extensions;
using Lotus.Roles.Interfaces;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.Extra;

public class GM : CustomRole, IPhantomRole
{
    public static Color GMColor = new(1f, 0.4f, 0.4f);

    protected override void PostSetup() => MyPlayer.RpcExileV2();

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream).Tab(DefaultTabs.HiddenTab);

    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier
        .RoleColor(GMColor)
        .Faction(FactionInstances.Solo)
        .RoleFlags(RoleFlag.Hidden | RoleFlag.Unassignable | RoleFlag.CannotWinAlone);

    public bool IsCountedAsPlayer() => false;
}