using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Factions;
using Lotus.Options;
using Lotus.Extensions;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.Extra;

public sealed class GM : CustomRole, IPhantomRole
{
    public static Color GMColor = new(1f, 0.4f, 0.4f);

    [RoleAction(RoleActionType.RoundStart)]
    public void ExileGM(bool roundStart)
    {
        if (!roundStart) return;
        MyPlayer.RpcExileV2(false);

        Players.GetPlayers().Where(p => p.PlayerId != MyPlayer.PlayerId)
            .SelectMany(p => p.NameModel().ComponentHolders())
            .ForEach(holders =>
                {
                    holders.AddListener(component => component.AddViewer(MyPlayer));
                    holders.Components().ForEach(components => components.AddViewer(MyPlayer));
                }
            );

        MyPlayer.NameModel().Render(force: true);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream).Tab(DefaultTabs.HiddenTab);

    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier
        .RoleColor(GMColor)
        .Faction(FactionInstances.Neutral)
        .RoleFlags(RoleFlag.Hidden | RoleFlag.Unassignable | RoleFlag.CannotWinAlone)
        .OptionOverride(Override.AnonymousVoting, false);

    public bool IsCountedAsPlayer() => false;
}