using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API;
using Lotus.API.Odyssey;
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
        Async.Schedule(() => BeginSuicideCountdown(target), 5f);
        return false;
    }

    
    [RoleAction(RoleActionType.AnyPlayerAction)]
    public void InterceptTargetAction(PlayerControl emitter, RoleAction action, ActionHandle handle, object[] parameters)
    {
        if (!manipulatedPlayers.Contains(emitter.PlayerId)) return;
        if (action.ActionType is not (RoleActionType.OnPet or RoleActionType.Attack)) return;
        
        PlayerControl? target = (PlayerControl?)(handle.ActionType is RoleActionType.Attack ? parameters[0] : emitter.GetPlayersInAbilityRangeSorted().FirstOrDefault());
        if (target == null) return;
        handle.Cancel();
        
        CustomRole emitterRole = emitter.GetCustomRole();
        Remote<GameOptionOverride> killCooldown =  emitterRole.AddOverride(new GameOptionOverride(Override.KillCooldown, 0));
        emitter.InteractWith(target, new ManipulatedInteraction(new FatalIntent(), emitter.GetCustomRole(), MyPlayer));
        killCooldown.Delete();
        emitterRole.SyncOptions();
        ClearManipulated(emitter);
    }

    private void BeginSuicideCountdown(PlayerControl target)
    {
        if (target == null) return;
        manipulatedPlayers.Add(target.PlayerId);
        Cooldown playerCooldown = expirationTimers[target.PlayerId] = timeToKill.Clone();
        LiveString killIndicator = new(ManipulatedText.Formatted(playerCooldown), RoleColor); 
        TextComponent textComponent = new(killIndicator, GameState.Roaming, viewers: target);
        remotes.GetOrCompute(target.PlayerId, () => new []{ (Remote<TextComponent>?)null, null})[1] = target.NameModel().GCH<TextHolder>().Add(textComponent);
        playerCooldown.StartThenRun(() => ExecuteSuicide(target));
    }

    private void ExecuteSuicide(PlayerControl target)
    {
        if (!manipulatedPlayers.Contains(target.PlayerId)) return;
        target.InteractWith(target, new UnblockedInteraction(new FatalIntent(false, () => new ManipulatedPlayerDeathEvent(target, target))));
        ClearManipulated(target);
    }
    
    
    [RoleAction(RoleActionType.MyDeath)]
    [RoleAction(RoleActionType.MeetingCalled)]
    public override void HandleDisconnect()
    {
        manipulatedPlayers.ToArray().Filter(Api.Players.PlayerById).ForEach(p =>
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
        base.RegisterOptions(optionStream)
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

    [Localized(nameof(Mastermind))]
    internal static class MastermindTranslations
    {
        [Localized(nameof(ManipulatedText))]
        public static string ManipulatedText = "Manipulated";

        [Localized(nameof(KillImploredText))]
        public static string KillImploredText = "You <b>MUST</b> Kill Someone In {0}s";

        [Localized("Options")]
        internal static class MastermindOptionTranslations
        {
            [Localized(nameof(ManipulatedPlayerLimit))]
            public static string ManipulatedPlayerLimit = "Manipulated Player Limit";

            [Localized(nameof(ImpostorsCanSeeManipulated))]
            public static string ImpostorsCanSeeManipulated = "Impostors::0 Can See Manipulated";

            [Localized(nameof(TimeUntilSuicide))]
            public static string TimeUntilSuicide = "Time Until Suicide";
        }
    }
}