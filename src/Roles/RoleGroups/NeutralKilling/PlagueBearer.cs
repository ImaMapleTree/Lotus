using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
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
using Lotus.Logging;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class PlagueBearer: NeutralKillingBase
{
    private static readonly Pestilence Pestilence = new();

    [NewOnSetup] private List<Remote<IndicatorComponent>> indicatorRemotes = new();
    [NewOnSetup] private HashSet<byte> infectedPlayers = null!;
    private int cooldownSetting;
    private float customCooldown;
    private int alivePlayers;

    public override bool CanSabotage() => false;

    protected override void PostSetup()
    {
        RelatedRoles.Add(typeof(Pestilence));
        CheckPestilenceTransform(new ActionHandle(RoleActionType.RoundStart));
    }

    [UIComponent(UI.Counter)]
    private string InfectionCounter() => RoleUtils.Counter(infectedPlayers.Count, alivePlayers, RoleColor);

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (infectedPlayers.Contains(target.PlayerId))
        {
            MyPlayer.RpcMark(target);
            return false;
        }

        if (MyPlayer.InteractWith(target, LotusInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return false;
        MyPlayer.RpcMark(target);

        DevLogger.Log(Translations.InfectedHistoryMessage);
        string historyMessage = Translations.InfectedHistoryMessage.Formatted(MyPlayer.name, target.name);
        historyMessage = TranslationUtil.Colorize(historyMessage, RoleColor, target.GetRoleColor());
        Game.MatchData.GameHistory.AddEvent(new GenericTargetedEvent(MyPlayer, target, historyMessage));

        infectedPlayers.Add(target.PlayerId);

        IndicatorComponent indicator = new SimpleIndicatorComponent("☀", RoleColor, Game.IgnStates, MyPlayer);
        indicatorRemotes.Add(target.NameModel().GetComponentHolder<IndicatorHolder>().Add(indicator));

        CheckPestilenceTransform(ActionHandle.NoInit());

        return false;
    }

    [RoleAction(RoleActionType.RoundStart)]
    [RoleAction(RoleActionType.RoundEnd)]
    [RoleAction(RoleActionType.AnyDeath)]
    [RoleAction(RoleActionType.Disconnect)]
    public void CheckPestilenceTransform(ActionHandle handle)
    {
        PlayerControl[] allCountedPlayers = GetAlivePlayers().ToArray();
        if (handle.ActionType is RoleActionType.RoundStart or RoleActionType.RoundEnd)
        {
            alivePlayers = allCountedPlayers.Length;
            infectedPlayers = infectedPlayers.Where(p => Players.PlayerById(p).Compare(o => o.IsAlive())).ToHashSet();
        }
        if (allCountedPlayers.Count(r => infectedPlayers.Contains(r.PlayerId)) != alivePlayers) return;

        indicatorRemotes.ForEach(remote => remote.Delete());
        MyPlayer.NameModel().GetComponentHolder<CounterHolder>().RemoveAt(0);
        MatchData.AssignRole(MyPlayer, Pestilence);

        Game.MatchData.GameHistory.AddEvent(new RoleChangeEvent(MyPlayer, Pestilence));
    }

    private IEnumerable<PlayerControl> GetAlivePlayers()
    {
        return Players.GetPlayers(PlayerFilter.Alive | PlayerFilter.NonPhantom)
            .Where(p => p.PlayerId != MyPlayer.PlayerId && Relationship(p) is not Relation.FullAllies);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream), key: "Infect Cooldown", name: Translations.Options.InfectCooldown);

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.9f, 1f, 0.7f))
            .RoleAbilityFlags(RoleAbilityFlag.CannotVent)
            .LinkedRoles(Pestilence)
            .OptionOverride(new IndirectKillCooldown(KillCooldown));

    [Localized(nameof(PlagueBearer))]
    private static class Translations
    {
        [Localized(nameof(InfectedHistoryMessage))]
        public static string InfectedHistoryMessage = "{0}::0 infected {1}::1.";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(InfectCooldown))]
            public static string InfectCooldown = "Infect Cooldown";
        }
    }

}
