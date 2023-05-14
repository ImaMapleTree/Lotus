using System.Linq;
using AmongUs.GameOptions;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.GUI.Name.Impl;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace Lotus.Roles.Subroles;

public class Psychopath: Subrole
{
    private static ColorGradient _psychoGradient = new(new Color(0.41f, 0.1f, 0.18f), new Color(0.75f, 0.35f, 0f));
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

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) => AddRestrictToCrew(base.RegisterOptions(optionStream));

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.41f, 0.1f, 0.18f));
}