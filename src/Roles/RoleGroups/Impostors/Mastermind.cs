using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using static Lotus.Roles.RoleGroups.Impostors.Mastermind.MastermindTranslations;
using static Lotus.Roles.RoleGroups.Impostors.Mastermind.MastermindTranslations.MastermindOptionTranslations;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Mastermind : Impostor
{
    private int manipulatedPlayerLimit;
    private bool impostorsCanSeeManipulated;
    private Cooldown timeToKill = null!;

    [NewOnSetup] private HashSet<byte> manipulatedPlayers = null!;
    [NewOnSetup] private Dictionary<byte, Remote<TextComponent>?[]> remotes = null!;
    [NewOnSetup] private Dictionary<byte, Cooldown> expirationTimers = null!;

    private bool CanManipulate => manipulatedPlayerLimit == -1 || manipulatedPlayers.Count < manipulatedPlayerLimit;
    
    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (!CanManipulate) return base.TryKill(target);

        PlayerControl[] viewers = impostorsCanSeeManipulated
            ? Game.GetAllPlayers().Where(p => !p.IsAlive() || Relationship(p) is Relation.FullAllies).AddItem(MyPlayer).ToArray()
            : new [] { MyPlayer };
        TextComponent alliedText = new(new LiveString(ManipulatedText, RoleColor), GameState.Roaming, ViewMode.Additive, viewers);

        Remote<TextComponent>?[] textComponents = { target.NameModel().GCH<TextHolder>().Add(alliedText), null };
        ClearManipulated(target);
        
        remotes[target.PlayerId] = textComponents;
        manipulatedPlayers.Add(target.PlayerId);
        
        Async.Schedule(() => BeginSuicideCountdown(target), 5f);
        RefreshKillCooldown(target);
        return false;
    }

    [RoleAction(RoleActionType.AnyPet)]
    public void InterceptPetAction(PlayerControl petter, ActionHandle handle)
    {
        if (!manipulatedPlayers.Contains(petter.PlayerId)) return;

        PlayerControl? target = petter.GetPlayersInAbilityRangeSorted().FirstOrDefault();
        if (target == null) return;
        handle.Cancel();
        DoManipulationKill(petter, target);
    }

    [RoleAction(RoleActionType.AnyPlayerAction)]
    public void InterceptTargetAction(PlayerControl emitter, RoleAction action, ActionHandle handle, object[] parameters)
    {
        if (!manipulatedPlayers.Contains(emitter.PlayerId)) return;
        if (action.ActionType is not RoleActionType.Attack) return;

        PlayerControl? target = (PlayerControl?)(handle.ActionType is RoleActionType.Attack ? parameters[0] : emitter.GetPlayersInAbilityRangeSorted().FirstOrDefault());
        if (target == null) return;
        handle.Cancel();
        DoManipulationKill(emitter, target);
    }

    private void DoManipulationKill(PlayerControl emitter, PlayerControl target)
    {
        CustomRole emitterRole = emitter.GetCustomRole();
        Remote<GameOptionOverride> killCooldown =  Game.MatchData.Roles.AddOverride(emitter.PlayerId, new GameOptionOverride(Override.KillCooldown, 0f));
        emitterRole.SyncOptions();
        Async.Schedule(() =>
        {
            emitter.InteractWith(target, new ManipulatedInteraction(new FatalIntent(), emitter.GetCustomRole(), MyPlayer));
            killCooldown.Delete();
            emitterRole.SyncOptions();
        }, NetUtils.DeriveDelay(0.05f));
        ClearManipulated(emitter);
    }

    private void BeginSuicideCountdown(PlayerControl target)
    {
        if (target == null) return;
        Cooldown playerCooldown = expirationTimers[target.PlayerId] = timeToKill.Clone();
        LiveString killIndicator = new(() => KillImploredText.Formatted(Color.white.Colorize(playerCooldown + "s")), RoleColor);
        
        TextComponent textComponent = new(killIndicator, GameState.Roaming, viewers: target);
        remotes.GetOrCompute(target.PlayerId, () => new []{ (Remote<TextComponent>?)null, null})[1] = target.NameModel().GCH<TextHolder>().Add(textComponent);
        playerCooldown.StartThenRun(() => ExecuteSuicide(target));
    }

    private void ExecuteSuicide(PlayerControl target)
    {
        if (!manipulatedPlayers.Contains(target.PlayerId)) return;
        target.InteractWith(target, new UnblockedInteraction(new FatalIntent(false, () => new ManipulatedPlayerDeathEvent(target, target)), this));
        ClearManipulated(target);
    }
    
    
    [RoleAction(RoleActionType.MyDeath)]
    [RoleAction(RoleActionType.MeetingCalled)]
    public override void HandleDisconnect()
    {
        manipulatedPlayers.ToArray().Filter(Players.PlayerById).ForEach(p =>
        {
            FatalIntent fatalIntent = new(false, () => new ManipulatedPlayerDeathEvent(p, p));
            p.InteractWith(p, new ManipulatedInteraction(fatalIntent, p.GetCustomRole(), MyPlayer));
            ClearManipulated(p);
        });
    }
    
    private void ClearManipulated(PlayerControl player)
    {
        remotes.GetValueOrDefault(player.PlayerId)?.ForEach(r => r?.Delete());
        manipulatedPlayers.Remove(player.PlayerId);
        expirationTimers.GetValueOrDefault(player.PlayerId)?.Finish();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) => 
        AddKillCooldownOptions(base.RegisterOptions(optionStream), ManipulationCooldown, "Manipulation Cooldown")
            .SubOption(sub => sub.KeyName("Manipulated Player Limit", ManipulatedPlayerLimit)
                .Value(v => v.Text(ModConstants.Infinity).Color(ModConstants.Palette.InfinityColor).Value(-1).Build())
                .AddIntRange(1, 5, 1, 0)
                .BindInt(i => manipulatedPlayerLimit = i)
                .Build())
            .SubOption(sub => sub.KeyName("Impostors Can See Manipulated", TranslationUtil.Colorize(ImpostorsCanSeeManipulated, RoleColor))
                .AddOnOffValues()
                .BindBool(b => impostorsCanSeeManipulated = b)
                .Build())
            .SubOption(sub => sub.KeyName("Time Until Suicide", TimeUntilSuicide)
                .Value(1f)
                .AddFloatRange(2.5f, 120, 2.5f, 5, "s")
                .BindFloat(timeToKill.SetDuration)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) => 
        base.Modify(roleModifier)
            .OptionOverride(new IndirectKillCooldown(KillCooldown, () => CanManipulate))
            .OptionOverride(Override.KillCooldown, () => AUSettings.KillCooldown(), () => !CanManipulate);

    [Localized(nameof(Mastermind))]
    internal static class MastermindTranslations
    {
        [Localized(nameof(ManipulatedText))]
        public static string ManipulatedText = "Manipulated!";

        [Localized(nameof(KillImploredText), ForceOverride = true)]
        public static string KillImploredText = "You <b>MUST</b> Kill Someone In {0}\n(Use either your Kill or Pet button!)";

        [Localized(ModConstants.Options)]
        internal static class MastermindOptionTranslations
        {
            [Localized(nameof(ManipulationCooldown))]
            public static string ManipulationCooldown = "Manipulation Cooldown";
            
            [Localized(nameof(ManipulatedPlayerLimit))]
            public static string ManipulatedPlayerLimit = "Manipulated Player Limit";

            [Localized(nameof(ImpostorsCanSeeManipulated))]
            public static string ImpostorsCanSeeManipulated = "Impostors::0 Can See Manipulated";

            [Localized(nameof(TimeUntilSuicide))]
            public static string TimeUntilSuicide = "Time Until Suicide";
        }
    }
}