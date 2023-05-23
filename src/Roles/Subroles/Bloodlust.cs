using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Extensions;
using Lotus.Roles.RoleGroups.Crew;
using Lotus.Roles.RoleGroups.Neutral;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.Subroles;

public class Bloodlust: Subrole
{
    private static Type[] _incompatibleRoles =
    {
        typeof(Crusader),
        typeof(Observer),
        typeof(Snitch),
        typeof(Speedrunner),
        typeof(Amnesiac),
        typeof(Copycat),
        typeof(Phantom)
    };
    
    private bool restrictedToCompatibleRoles;
    private static ColorGradient _psychoGradient = new(new Color(0.41f, 0.1f, 0.18f), new Color(0.85f, 0.77f, 0f));
    private bool requiresBaseKillMethod;
    

    public override string Identifier() => "";

    [RoleAction(RoleActionType.Attack)]
    private bool TryKill(PlayerControl target)
    {
        if (!requiresBaseKillMethod) return false;
        InteractionResult result = MyPlayer.InteractWith(target, DirectInteraction.FatalInteraction.Create(this));
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, target, result is InteractionResult.Proceed));
        return result is InteractionResult.Proceed;
    }

    protected override void PostSetup()
    {
        CustomRole role = MyPlayer.GetCustomRole();
        RoleHolder roleHolder = MyPlayer.NameModel().GetComponentHolder<RoleHolder>();
        string newRoleName = _psychoGradient.Apply(role.RoleName);
        roleHolder.Add(new RoleComponent(new LiveString(newRoleName), GameStates.IgnStates, ViewMode.Replace, MyPlayer));
        role.Faction = FactionInstances.Solo;
        if (role.RealRole.IsCrewmate())
        {
            role.DesyncRole = RoleTypes.Impostor;
            MyPlayer.GetTeamInfo().MyRole = role.DesyncRole.Value;
        }
        requiresBaseKillMethod = !role.GetActions(RoleActionType.Attack).Any();
    }

    public override HashSet<Type>? RestrictedRoles()
    {
        HashSet<Type>? restrictedRoles = base.RestrictedRoles();
        if (!restrictedToCompatibleRoles) return restrictedRoles;
        _incompatibleRoles.ForEach(r => restrictedRoles?.Add(r));
        return restrictedRoles;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) => 
        AddRestrictToCrew(base.RegisterOptions(optionStream))
            .SubOption(sub => sub.KeyName("Restrict to Compatible Roles", Translations.Options.RestrictToCompatbileRoles)
                .BindBool(b => restrictedToCompatibleRoles = b)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.41f, 0.1f, 0.18f));

    [Localized(nameof(Bloodlust))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        internal static class Options
        {
            public static string RestrictToCompatbileRoles = "Restrict to Compatible Roles";
        }
    }
}