using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Stats;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Factions.Crew;
using Lotus.Factions.Interfaces;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Logging;
using Lotus.Managers.History.Events;
using Lotus.Options;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.Victory;
using Lotus.Victory.Conditions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Crew;

public class Charmer: Crewmate
{
    private static IAccumulativeStatistic<int> _charmedPlayers = Statistic<int>.CreateAccumulative($"Roles.{nameof(Charmer)}.CharmedPlayers", () => Translations.CharmedStatistic);
    public static List<Statistic> AlurerStatistics = new() { _charmedPlayers };
    public override List<Statistic> Statistics() => AlurerStatistics;

    private static Color _charmedColor = new(1f, 0.49f, 0.62f);
    private static Charmed _charmedFaction = new();
    private bool usesKillButton;
    private int tasksPerUsage;
    private bool charmedPlayersWinWithCrew;
    private bool breakCharmOnDeath;
    private int maxCharmedPlayers;

    private int taskAbilityCount;

    [UIComponent(UI.Cooldown)]
    public Cooldown charmingCooldown;


    [NewOnSetup] private Dictionary<byte, (Remote<NameComponent>, IFaction)> charmedPlayers = new();

    protected override void PostSetup()
    {
        if (charmedPlayersWinWithCrew) Game.GetWinDelegate().AddSubscriber(CharmedWinWithCrew);
        if (tasksPerUsage == 0) return;
        LiveString taskCounter = new(() => RoleUtils.Counter(taskAbilityCount, tasksPerUsage, RoleColor), Color.white);
        MyPlayer.NameModel().GCH<CounterHolder>().Add(new CounterComponent(taskCounter, Game.IgnStates, ViewMode.Additive, MyPlayer));
    }

    public void CharmedWinWithCrew(WinDelegate winDelegate)
    {
        if (winDelegate.WinCondition() is not IFactionWinCondition factionWinCondition) return;
        if (factionWinCondition.Factions().All(f => f is not Crewmates)) return;
        winDelegate.GetWinners().AddRange(charmedPlayers.Keys.Filter(Players.PlayerById));
    }

    [RoleAction(RoleActionType.Attack)]
    public bool CharmPlayer(PlayerControl player)
    {
        if (taskAbilityCount < tasksPerUsage) return false;
        if (maxCharmedPlayers > 0 && charmedPlayers.Count >= maxCharmedPlayers) return false;

        if (Relationship(player) is Relation.FullAllies)
        {
            IDeathEvent suicide = new MisfiredEvent(MyPlayer);
            MyPlayer.InteractWith(MyPlayer, new UnblockedInteraction(new FatalIntent(false, () => suicide), this));
            return false;
        }

        taskAbilityCount = 0;
        MyPlayer.RpcMark(player);
        if (MyPlayer.InteractWith(player, DirectInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return true;

        LiveString charmString = new(Translations.CharmedText, _charmedColor);
        NameComponent component = new(charmString, GameStates.IgnStates, ViewMode.Additive, MyPlayer, player);

        CustomRole playerRole = player.GetCustomRole();
        charmedPlayers[player.PlayerId] = (player.NameModel().GCH<NameHolder>().Insert(0, component), playerRole.Faction);
        playerRole.Faction = _charmedFaction;

        return true;
    }

    [RoleAction(RoleActionType.OnPet)]
    public void CharmPlayerPet()
    {
        if (usesKillButton) return;
        if (charmingCooldown.NotReady()) return;
        PlayerControl? closestPlayer = MyPlayer.GetPlayersInAbilityRangeSorted().FirstOrDefault();
        if (closestPlayer == null) return;
        if (charmedPlayers.ContainsKey(closestPlayer.PlayerId)) return;
        if (CharmPlayer(closestPlayer)) charmingCooldown.Start();
    }

    [RoleAction(RoleActionType.AnyInteraction, triggerAfterDeath: true)]
    public void PreventCrewmateInteractions(PlayerControl actor, PlayerControl target, Interaction interaction, ActionHandle handle)
    {
        if (breakCharmOnDeath && !MyPlayer.IsAlive()) return;
        if (!charmedPlayers.ContainsKey(actor.PlayerId)) return;
        if (interaction.Intent() is not (IFatalIntent or IHostileIntent)) return;
        if (Relationship(target) is not Relation.FullAllies) return;
        handle.Cancel();
    }

    [RoleAction(RoleActionType.MyDeath)]
    public override void HandleDisconnect()
    {
        if (!breakCharmOnDeath) return;
        charmedPlayers.Keys.ToArray().Filter(Players.PlayerById).ForEach(CheckPlayerDeathAndDisconnect);
    }

    [RoleAction(RoleActionType.AnyDeath)]
    [RoleAction(RoleActionType.Disconnect)]
    public void CheckPlayerDeathAndDisconnect(PlayerControl player)
    {
        if (!charmedPlayers.Remove(player.PlayerId, out (Remote<NameComponent>, IFaction) tuple)) return;
        tuple.Item1.Delete();
        player.GetCustomRole().Faction = tuple.Item2;
    }

    protected override void OnTaskComplete(Optional<NormalPlayerTask> _) => tasksPerUsage++;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Ability Button", Translations.Options.AbilityButton)
                .Value(v => v.Text(Translations.Options.PetButton).Value(false).Color(ModConstants.Palette.PassiveColor).Build())
                .Value(v => v.Text(Translations.Options.KillButton).Value(true).Color(ModConstants.Palette.KillingColor).Build())
                .BindBool(b => usesKillButton = b)
                .Build())
            .SubOption(sub => sub.KeyName("Tasks Needed for Ability", Translations.Options.TasksPerAbility)
                .AddIntRange(0, 10, 1, 0)
                .BindInt(i => tasksPerUsage = i)
                .Build())
            .SubOption(sub2 => sub2.Name(Translations.Options.CharmingCooldown)
                .Key("Charming Cooldown")
                .Value(v => v.Text(GeneralOptionTranslations.GlobalText).Color(new Color(1f, 0.61f, 0.33f)).Value(-1).Build())
                .AddFloatRange(0, 120, 2.5f, 25, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(charmingCooldown.SetDuration)
                .Build())
            .SubOption(sub => sub.KeyName("Charmed Players Win with Crew", Translations.Options.CharmedPlayersWinWithCrew)
                .BindBool(b => charmedPlayersWinWithCrew = b)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub.KeyName("Break Charm on Charmer Death", TranslationUtil.Colorize(Translations.Options.BreakCharmOnDeath, RoleColor))
                .BindBool(b => breakCharmOnDeath = b)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub.KeyName("Max Charmed Players", Translations.Options.MaxCharmedPlayers)
                .BindInt(i => maxCharmedPlayers = i)
                .Value(v => v.Text(ModConstants.Infinity).Color(ModConstants.Palette.InfinityColor).Value(0).Build())
                .AddIntRange(1, 15, 1, 0)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.71f, 0.67f, 0.9f))
            .DesyncRole(usesKillButton ? RoleTypes.Impostor : RoleTypes.Crewmate)
            .OptionOverride(Override.ImpostorLightMod, () => AUSettings.CrewLightMod())
            .OptionOverride(new IndirectKillCooldown(() => charmingCooldown.Duration <= -1 ? AUSettings.KillCooldown() : charmingCooldown.Duration));

    private class Charmed : Faction<Charmed>
    {
        public override Relation Relationship(Charmed sameFaction) => Relation.None;

        public override bool CanSeeRole(PlayerControl player) => false;

        public override Color FactionColor() => _charmedColor;

        public override Relation RelationshipOther(IFaction other) => other is Crewmates ? Relation.SharedWinners : Relation.None;
    }


    [Localized(nameof(Charmer))]
    private static class Translations
    {
        [Localized(nameof(CharmedText))]
        public static string CharmedText = "Charmed";

        [Localized(nameof(CharmedStatistic))]
        public static string CharmedStatistic = "Charmed Players";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(AbilityButton))]
            public static string AbilityButton = "Ability Button";

            [Localized(nameof(KillButton))]
            public static string KillButton = "Kill Button";

            [Localized(nameof(PetButton))]
            public static string PetButton = "Pet Button";

            [Localized(nameof(TasksPerAbility))]
            public static string TasksPerAbility = "Tasks Needed for Ability";

            [Localized(nameof(CharmingCooldown))]
            public static string CharmingCooldown = "Charming Cooldown";

            [Localized(nameof(CharmedPlayersWinWithCrew))]
            public static string CharmedPlayersWinWithCrew = "Charmed Players Win with Crew";

            [Localized(nameof(BreakCharmOnDeath))]
            public static string BreakCharmOnDeath = "Break Charm on Charmer::0 Death";

            [Localized(nameof(MaxCharmedPlayers))]
            public static string MaxCharmedPlayers = "Max Charmed Players";
        }
    }
}