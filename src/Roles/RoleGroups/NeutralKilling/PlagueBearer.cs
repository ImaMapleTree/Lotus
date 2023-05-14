using System.Collections.Generic;
using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Managers.History.Events;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Utilities;
using Lotus.Extensions;
using Lotus.Factions.Neutrals;
using Lotus.Options;
using Lotus.Options.Roles;
using UnityEngine;
using VentLib.Options;
using VentLib.Options.Events;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Optionals;
using static Lotus.Managers.CustomRoleManager;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class PlagueBearer: NeutralKillingBase
{
    [NewOnSetup] private List<Remote<IndicatorComponent>> indicatorRemotes = new();
    [NewOnSetup] private HashSet<byte> infectedPlayers;
    private int cooldownSetting;
    private float customCooldown;
    private int alivePlayers;

    public override bool CanSabotage() => false;

    protected override void PostSetup() => RelatedRoles.Add(typeof(Pestilence));

    [UIComponent(UI.Counter)]
    private string InfectionCounter() => RoleUtils.Counter(infectedPlayers.Count, alivePlayers, RoleColor);

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (infectedPlayers.Contains(target.PlayerId))
        {
            MyPlayer.RpcGuardAndKill(target);
            return false;
        }

        if (MyPlayer.InteractWith(target, DirectInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return false;
        MyPlayer.RpcGuardAndKill(target);

        Game.MatchData.GameHistory.AddEvent(new GenericTargetedEvent(MyPlayer, target, $"{MyPlayer.name} infected {target.name}."));

        infectedPlayers.Add(target.PlayerId);

        IndicatorComponent indicator = new SimpleIndicatorComponent("◆", RoleColor, GameStates.IgnStates, MyPlayer);
        indicatorRemotes.Add(target.NameModel().GetComponentHolder<IndicatorHolder>().Add(indicator));

        CheckPestilenceTransform();

        return false;
    }

    [RoleAction(RoleActionType.RoundStart)]
    [RoleAction(RoleActionType.RoundEnd)]
    [RoleAction(RoleActionType.AnyDeath)]
    public void CheckPestilenceTransform(ActionHandle? handle = null)
    {
        handle ??= ActionHandle.NoInit();
        CustomRole[] aliveRoles = Game.GetAliveRoles()
            .Where(r => !ReferenceEquals(r, this))
            .Where(r => Relationship(r) is not Relation.FullAllies)
            .ToArray();
        if (handle.ActionType is RoleActionType.RoundStart or RoleActionType.RoundEnd) alivePlayers = aliveRoles.Length;
        if (aliveRoles.Count(r => infectedPlayers.Contains(r.MyPlayer.PlayerId)) != alivePlayers) return;

        indicatorRemotes.ForEach(remote => remote.Delete());
        MyPlayer.NameModel().GetComponentHolder<CounterHolder>().RemoveAt(0);
        Api.Roles.AssignRole(MyPlayer, Static.Pestilence);

        Game.MatchData.GameHistory.AddEvent(new RoleChangeEvent(MyPlayer, Static.Pestilence));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Infect Cooldown")
                .Value(v => v.Text("Default Kill CD").Value(0).Build())
                .Value(v => v.Text("Half Default Kill CD").Value(1).Build())
                .Value(v => v.Text("Custom CD").Value(2).Build())
                .BindInt(i => cooldownSetting = i)
                .ShowSubOptionPredicate(o => (int)o == 2)
                .SubOption(sub2 => sub2
                    .Name("Custom Infect Cooldown")
                    .AddFloatRange(5f, 120f, 2.5f, 8, "s")
                    .BindFloat(f => customCooldown = f)
                    .Build())
                .Build())
            .SubOption(sub => sub
                .Name("Pestilence Settings")
                .Color(new Color(0.22f, 0.22f, 0.22f))
                .Value(v => v.Text("Show").Color(Color.cyan).Value(true).Build())
                .Value(v => v.Text("Hide").Color(Color.red).Value(false).Build())
                .BindEvent(ev =>
                {
                    OptionHelpers.GetChildren(ev.Source(), true).ForEach(opt => opt.NotifySubscribers(new OptionValueEvent(opt, new Optional<object>(opt.GetValue()), opt.GetValue())));
                    if (!(bool)ev.NewValue()) Utils.RunUntilSuccess(() => Static.Pestilence.SetDefaultSettings(), 1f);
                })
                .ShowSubOptionPredicate(o => (bool)o)
                .SubOption(sub2 => sub2
                    .Name("Unblockable Kill")
                    .AddOnOffValues(false)
                    .BindBool(b => Utils.RunUntilSuccess(() => Static.Pestilence.UnblockableAttacks = b, 1f))
                    .Build())
                .SubOption(sub2 => sub2
                    .Name("Invincibility Settings")
                    .Value(v => v.Text("Default").Value(false).Color(Color.cyan).Build())
                    .Value(v => v.Text("Custom").Value(true).Color(new(0.45f, 0.31f, 0.72f)).Build())
                    .ShowSubOptionPredicate(o => (bool)o)
                    .SubOption(sub3 =>  sub3
                        .Name("Immune to Manipulated Attackers")
                        .AddOnOffValues(false)
                        .BindBool(b => Utils.RunUntilSuccess(() => Static.Pestilence.ImmuneToManipulated = b, 1f))
                        .Build())
                    .SubOption(sub3 => sub3
                        .Name("Immune to Ranged Attacks")
                        .AddOnOffValues(false)
                        .BindBool(b => Utils.RunUntilSuccess(() => Static.Pestilence.ImmuneToRangedAttacks = b, 1f))
                        .Build())
                    .SubOption(sub3 => sub3
                        .Name("Immune to Delayed Attacks")
                        .AddOnOffValues(false)
                        .BindBool(b => Utils.RunUntilSuccess(() => Static.Pestilence.ImmuneToDelayedAttacks = b, 1f))
                        .Build())
                    .SubOption(sub3 => sub3
                        .Name("Immune to Arsonist Ignite")
                        .AddOnOffValues(false)
                        .BindBool(b => Utils.RunUntilSuccess(() => Static.Pestilence.ImmuneToArsonist = b, 1f))
                        .Build())
                    .Build())
                .Build());


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.9f, 1f, 0.7f))
            .CanVent(false)
            .OptionOverride(new IndirectKillCooldown(KillCooldown, () => cooldownSetting == 0))
            .OptionOverride(new IndirectKillCooldown(KillCooldown / 2, () => cooldownSetting == 1))
            .OptionOverride(new IndirectKillCooldown(customCooldown, () => cooldownSetting == 2));


    private class InfectEvent : TargetedAbilityEvent
    {
        public InfectEvent(PlayerControl source, PlayerControl target, bool successful = true) : base(source, target, successful)
        {
        }

        public override string Message() => $"{Game.GetName(Player())} infected {Game.GetName(Target())}";
    }
}
