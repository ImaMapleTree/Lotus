using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Options;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.Victory;
using TOHTOR.Victory.Conditions;
using UnityEngine;
using VentLib.Options.Game;

namespace TOHTOR.Roles.RoleGroups.Neutral;

public class Opportunist : Crewmate
{
    protected override void PostSetup() => Game.GetWinDelegate().AddSubscriber(WinSubscriber);

    private void WinSubscriber(WinDelegate winDelegate)
    {
        if (!MyPlayer.IsAlive() || winDelegate.GetWinReason() is WinReason.SoloWinner) return;
        winDelegate.GetWinners().Add(MyPlayer);
    }


    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream).Tab(DefaultTabs.NeutralTab);


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(Color.green).SpecialType(SpecialType.Neutral).Faction(FactionInstances.Solo);
}