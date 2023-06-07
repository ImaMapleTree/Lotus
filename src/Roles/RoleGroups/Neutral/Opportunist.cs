using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Options;
using Lotus.Roles.Internals;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Victory;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Opportunist : Crewmate
{
    public override bool TasksApplyToTotal() => false;
    
    protected override void PostSetup() => Game.GetWinDelegate().AddSubscriber(WinSubscriber);

    private void WinSubscriber(WinDelegate winDelegate)
    {
        if (MyPlayer != null && MyPlayer.IsAlive()) winDelegate.GetWinners().Add(MyPlayer);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream).Tab(DefaultTabs.NeutralTab);

    protected override RoleModifier Modify(RoleModifier roleModifier) => 
        base.Modify(roleModifier).RoleColor(Color.green)
            .SpecialType(SpecialType.Neutral)
            .Faction(FactionInstances.Neutral)
            .RoleFlags(RoleFlag.CannotWinAlone);
}