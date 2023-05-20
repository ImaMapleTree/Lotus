using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.API;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using static Lotus.ModConstants;
using static Lotus.Roles.RoleGroups.Crew.Bodyguard.BodyguardTranslations;
using static Lotus.Roles.RoleGroups.Crew.Bodyguard.BodyguardTranslations.BodyguardOptions;
using static Lotus.Utilities.TranslationUtil;

namespace Lotus.Roles.RoleGroups.Crew;

public class Bodyguard: Crewmate
{
    private Optional<byte> guardedPlayer = Optional<byte>.Null();
    private bool protectAgainstHelpfulInteraction;
    private bool protectAgainstNeutralInteraction;
    private GuardMode guardMode;
    private bool castVote;
    private bool guardedOnce;

    [RoleAction(RoleActionType.RoundStart)]
    private void SwapGuard()
    {
        guardedOnce = guardedOnce || guardedPlayer.Exists();
        guardedPlayer.IfPresent(player =>
        {
            Game.MatchData.GameHistory.AddEvent(new ProtectEvent(MyPlayer, Utils.GetPlayerById(player)!));
        });
    }

    [RoleAction(RoleActionType.RoundEnd)]
    private void SendAndCheckGuarded()
    {
        castVote = false;
        guardedPlayer.IfPresent(b =>
        {
            if (Game.GetAlivePlayers().All(p => p.PlayerId != b)) guardedPlayer = Optional<byte>.Null();
        });

        Utils.SendMessage($"{ProtectingMessage} {guardedPlayer.FlatMap(GetPlayerName).OrElse("No One")}\n{VotePlayerMessage}", MyPlayer.PlayerId);
    }

    [RoleAction(RoleActionType.MyVote)]
    private void ProtectPlayer(Optional<PlayerControl> votedPlayer, ActionHandle handle)
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (guardedOnce && guardMode is GuardMode.Never) return;
        if (guardedOnce && guardedPlayer.Exists() && guardMode is GuardMode.OnDeath) return;
        if (castVote) return;
        castVote = true;
        handle.Cancel();
        if (!votedPlayer.Exists()) return;
        byte voted = votedPlayer.Get().PlayerId;

        if (MyPlayer.PlayerId == voted) { }
        else if (!guardedPlayer.Exists()) guardedPlayer = votedPlayer.Map(p => p.PlayerId);
        else guardedPlayer = guardedPlayer.Exists() ? new Optional<byte>() : new Optional<byte>(guardedPlayer.Get());

        Utils.SendMessage($"{ProtectingMessage} {guardedPlayer.FlatMap(GetPlayerName).OrElse("No One")}", MyPlayer.PlayerId);
    }

    [RoleAction(RoleActionType.AnyInteraction)]
    private void AnyPlayerInteraction(PlayerControl actor, PlayerControl target, Interaction interaction, ActionHandle handle)
    {
        Intent intent = interaction.Intent();
        if (Game.State is not GameState.Roaming) return;
        if (!guardedPlayer.Exists() || target.PlayerId != guardedPlayer.Get()) return;

        switch (intent)
        {
            case IHelpfulIntent when !protectAgainstHelpfulInteraction:
            case INeutralIntent when !protectAgainstNeutralInteraction:
            case IFatalIntent fatalIntent when fatalIntent.IsRanged():
                return;
        }

        if (interaction is IDelayedInteraction or IRangedInteraction or IIndirectInteraction) return;
        handle.Cancel();

        RoleUtils.SwapPositions(target, MyPlayer);
        Game.MatchData.GameHistory.AddEvent(new PlayerSavedEvent(target, MyPlayer, actor));
        InteractionResult result = MyPlayer.InteractWith(actor, DirectInteraction.FatalInteraction.Create(this));

        if (result is InteractionResult.Proceed) Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, actor));
        
        
        actor.GetCustomRole().GetActions(RoleActionType.Attack)
            .FirstOrOptional()
            .Handle(t => t.Item1.Execute(t.Item2, new object[] { MyPlayer} ),
            () =>
            {
                if (actor.InteractWith(MyPlayer, DirectInteraction.FatalInteraction.Create(this)) is InteractionResult.Proceed)
                    Game.MatchData.GameHistory.AddEvent(new KillEvent(actor, MyPlayer));
            });
    }
    
    [RoleAction(RoleActionType.Disconnect)]
    private void HandleDisconnect(PlayerControl player)
    {
        if (!guardedPlayer.Exists() || guardedPlayer.Get() != player.PlayerId) return;
        guardedPlayer = Optional<byte>.Null();
        guardedOnce = false;
    }

    private static Optional<string> GetPlayerName(byte b) => Game.GetAlivePlayers().FirstOrOptional(p => p.PlayerId == b).Map(p => p.name);

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.25f, 0.36f, 0.3f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Change Guarded Player")
                .Value(v => v.Text("When Guarded Player Dies").Value(0).Build())
                .Value(v => v.Text("Any Meeting").Value(1).Build())
                .Value(v => v.Text("Never").Value(2).Build())
                .BindInt(o => guardMode = (GuardMode)o)
                .Build())
            .SubOption(sub => sub.KeyName("Protect against Beneficial Interactions", Colorize(BeneficialInteractionProtection, ModConstants.Palette.PassiveColor))
                .BindBool(b => protectAgainstHelpfulInteraction = b)
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub.KeyName("Protect against Neutral Interactions", Colorize(NeutralInteractionProtection, ModConstants.Palette.NeutralColor))
                .BindBool(b => protectAgainstNeutralInteraction = b)
                .AddOnOffValues()
                .Build());
            /*.SubOption(sub => sub.Name(""));*/

    private enum GuardMode
    {
        OnDeath,
        PerRound,
        Never
    }

    [Localized(nameof(Bodyguard))]
    internal static class BodyguardTranslations
    {
        [Localized(nameof(ProtectingMessage))]
        public static string ProtectingMessage = "You are currently protecting:";

        [Localized("VotePlayerInfo")]
        public static string VotePlayerMessage = "Vote to select a player to guard.";
        
        [Localized(ModConstants.Options)]
        public static class BodyguardOptions
        {
            [Localized(nameof(BeneficialInteractionProtection))]
            public static string BeneficialInteractionProtection = "Protect against Beneficial::0 Interactions";
            
            [Localized(nameof(NeutralInteractionProtection))]
            public static string NeutralInteractionProtection = "Protect against Neutral::0 Interactions";
        }
    }
}