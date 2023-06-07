using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Stats;
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
using Lotus.Utilities;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.NeutralKilling;

public class Arsonist : NeutralKillingBase
{
    private static IAccumulativeStatistic<int> _dousedPlayers = Statistic<int>.CreateAccumulative($"Roles.{nameof(Arsonist)}.DousedPlayers", () => Translations.DousedPlayerStatistic);
    private static IAccumulativeStatistic<int> _incineratedPlayers = Statistic<int>.CreateAccumulative($"Roles.{nameof(Arsonist)}.IncineratedPlayers", () => Translations.IncineratedPlayerStatistic);

    public override List<Statistic> Statistics()
    {
        if (MyPlayer == null) return new List<Statistic> { _incineratedPlayers, _dousedPlayers };
        if (_incineratedPlayers.GetValue(MyPlayer.UniquePlayerId()) >= _dousedPlayers.GetValue(MyPlayer.UniquePlayerId())) return new List<Statistic> { _incineratedPlayers, _dousedPlayers };
        return new List<Statistic> { _dousedPlayers, _incineratedPlayers };
    }

    private static string[] _douseProgressIndicators = { "◦", "◎", "◉", "●" };

    private int requiredAttacks;
    private bool canIgniteAnyitme;

    private int knownAlivePlayers;
    [NewOnSetup] private HashSet<byte> dousedPlayers;
    [NewOnSetup] private Dictionary<byte, Remote<IndicatorComponent>> indicators;
    [NewOnSetup] private Dictionary<byte, int> douseProgress;

    [UIComponent(UI.Counter)]
    private string DouseCounter() => RoleUtils.Counter(dousedPlayers.Count, knownAlivePlayers);

    [UIComponent(UI.Text)]
    private string DisplayWin() => dousedPlayers.Count >= knownAlivePlayers ? RoleColor.Colorize(Translations.PressIgniteToWinMessage) : "";

    [RoleAction(RoleActionType.Attack)]
    public new bool TryKill(PlayerControl target)
    {
        bool douseAttempt = MyPlayer.InteractWith(target, DirectInteraction.HostileInteraction.Create(this)) is InteractionResult.Proceed;
        if (!douseAttempt) return false;

        int progress = douseProgress[target.PlayerId] = douseProgress.GetValueOrDefault(target.PlayerId) + 1;
        if (progress > requiredAttacks) return false;

        RenderProgress(target, progress);
        if (progress < requiredAttacks) return false;

        dousedPlayers.Add(target.PlayerId);
        MyPlayer.RpcMark(target);
        Game.MatchData.GameHistory.AddEvent(new GenericTargetedEvent(MyPlayer, target, Translations.DouseEventMessage.Formatted(MyPlayer.name, target.name)));
        _dousedPlayers.Update(MyPlayer.UniquePlayerId(), i => i + 1);

        MyPlayer.NameModel().Render();

        return false;
    }

    private void RenderProgress(PlayerControl target, int progress)
    {
        if (progress > requiredAttacks) return;
        string indicator = _douseProgressIndicators[Mathf.Clamp(Mathf.FloorToInt(progress / (requiredAttacks / (float)_douseProgressIndicators.Length) - 1), 0, 3)];

        Remote<IndicatorComponent> IndicatorSupplier() => target.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent("", GameStates.IgnStates, viewers: MyPlayer));

        Remote<IndicatorComponent> component = indicators.GetOrCompute(target.PlayerId, IndicatorSupplier);
        component.Get().SetMainText(new LiveString(indicator, RoleColor));
    }


    [RoleAction(RoleActionType.OnPet)]
    private void KillDoused() => dousedPlayers.Filter(p => Utils.PlayerById(p)).Where(p => p.IsAlive()).Do(p =>
    {
        if (dousedPlayers.Count < knownAlivePlayers && !canIgniteAnyitme) return;
        FatalIntent intent = new(true, () => new CustomDeathEvent(p, MyPlayer, Translations.IncineratedDeathName));
        IndirectInteraction interaction = new(intent, this);
        MyPlayer.InteractWith(p, interaction);
        _incineratedPlayers.Update(MyPlayer.UniquePlayerId(), i => i + 1);
    });

    [RoleAction(RoleActionType.RoundStart)]
    protected override void PostSetup()
    {
        knownAlivePlayers = Game.GetAlivePlayers().Count(p => p.PlayerId != MyPlayer.PlayerId && Relationship(p) is not Relation.FullAllies);
        dousedPlayers.RemoveWhere(p => Utils.PlayerById(p).Transform(pp => !pp.IsAlive(), () => true));
    }

    [RoleAction(RoleActionType.MyDeath)]
    private void ArsonistDies() => indicators.Values.ForEach(v => v.Delete());

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Color(RoleColor)
            .SubOption(sub => sub.KeyName("Attacks to Complete Douse", Translations.Options.AttacksToCompleteDouse)
                .AddIntRange(3, 100, defaultIndex: 16)
                .BindInt(i => requiredAttacks = i)
                .Build())
            .SubOption(sub => sub.KeyName("Can Ignite Anytime", Translations.Options.CanIgniteAnytime)
                .AddOnOffValues(false)
                .BindBool(b => canIgniteAnyitme = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(1f, 0.4f, 0.2f))
            .RoleAbilityFlags(RoleAbilityFlag.CannotSabotage | RoleAbilityFlag.CannotVent);

    [Localized(nameof(Arsonist))]
    private static class Translations
    {
        [Localized(nameof(IncineratedDeathName))]
        public static string IncineratedDeathName = "Incinerated";

        [Localized(nameof(DouseEventMessage))]
        public static string DouseEventMessage = "{0} doused {1}.";

        [Localized(nameof(PressIgniteToWinMessage))]
        public static string PressIgniteToWinMessage = "Press Ignite to Win";

        [Localized(nameof(DousedPlayerStatistic))]
        public static string DousedPlayerStatistic = "Doused Players";

        [Localized(nameof(IncineratedPlayerStatistic))]
        public static string IncineratedPlayerStatistic = "Incinerated Players";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(AttacksToCompleteDouse))]
            public static string AttacksToCompleteDouse = "Attacks to Complete Douse";

            [Localized(nameof(CanIgniteAnytime))]
            public static string CanIgniteAnytime = "Can Ignite Anytime";
        }
    }
}