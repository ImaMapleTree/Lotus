using AmongUs.GameOptions;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.Managers;
using Lotus.Options;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Impostors;
using Lotus.Extensions;
using Lotus.Roles.Legacy;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Amnesiac : CustomRole
{
    private bool stealExactRole;

    [RoleAction(RoleActionType.AnyReportedBody)]
    public void AmnesiacRememberAction(PlayerControl reporter, GameData.PlayerInfo reported, ActionHandle handle)
    {
        VentLogger.Trace($"Reporter: {reporter.name} | Reported: {reported.GetNameWithRole()} | Self: {MyPlayer.name}", "");

        if (reporter.PlayerId != MyPlayer.PlayerId) return;
        CustomRole newRole = reported.GetCustomRole();
        if (!stealExactRole)
        {
            if (newRole.SpecialType == SpecialType.NeutralKilling) { }
            else if (newRole.SpecialType == SpecialType.Neutral)
                newRole = CustomRoleManager.Static.Opportunist;
            else if (newRole.IsCrewmate())
                newRole = CustomRoleManager.Static.Sheriff;
            else
                newRole = Ref<Traitor>();
        }

        MatchData.AssignRole(MyPlayer, newRole);

        CustomRole role = MyPlayer.GetCustomRole();
        role.DesyncRole = RoleTypes.Impostor;

        handle.Cancel();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => sub.Name("Steals Exact Role")
                .Bind(v => stealExactRole = (bool)v)
                .AddOnOffValues(false).Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(0.51f, 0.87f, 0.99f))
            .RoleAbilityFlags(RoleAbilityFlag.CannotSabotage | RoleAbilityFlag.CannotVent)
            .SpecialType(SpecialType.Neutral)
            .DesyncRole(RoleTypes.Impostor)
            .Faction(FactionInstances.Solo);
    
}