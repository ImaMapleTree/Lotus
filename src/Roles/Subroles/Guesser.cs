using System;
using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Managers;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Trackers;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.Subroles;

public class Guesser: CustomRole
{
    private MeetingPlayerSelector voteSelector = new();

    private bool hasMadeGuess;
    private byte guessingPlayer = byte.MaxValue;
    private bool skippedVote;
    private CustomRole? guessedRole;
    private string? guesserMessage;

    [RoleAction((RoleActionType.RoundStart))]
    [RoleAction((RoleActionType.RoundEnd))]
    public void ResetPreppedPlayer()
    {
        hasMadeGuess = false;
        voteSelector.Reset();
        guessingPlayer = byte.MaxValue;
        skippedVote = false;
        guessedRole = null;
        guesserMessage = null;
    }

    [RoleAction(RoleActionType.MyVote)]
    public void SelectPlayerToGuess(Optional<PlayerControl> player, ActionHandle handle)
    {
        if (skippedVote || hasMadeGuess) return;
        handle.Cancel();
        VoteResult result = voteSelector.CastVote(player);
        switch (result.VoteResultType)
        {
            case VoteResultType.None:
                break;
            case VoteResultType.Skipped:
                skippedVote = true;
                break;
            case VoteResultType.Selected:
                guessingPlayer = result.Selected;
                GuesserHandler(Translations.PickedPlayerText.Formatted(Players.FindPlayerById(result.Selected)?.name)).Send(MyPlayer);
                break;
            case VoteResultType.Confirmed:
                if (guessedRole == null)
                {
                    voteSelector.Reset();
                    voteSelector.CastVote(player);
                } else hasMadeGuess = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (!hasMadeGuess) return;
        PlayerControl? guessed = Players.FindPlayerById(guessingPlayer);
        if (guessed == null || guessedRole == null)
        {
            GuesserHandler(Translations.ErrorCompletingGuess).Send(MyPlayer);
            ResetPreppedPlayer();
            return;
        }

        if (guessed.GetCustomRole().GetType() == guessedRole.GetType())
        {
            guesserMessage = Translations.GuessAnnouncementMessage.Formatted(guessed.name);
            MyPlayer.InteractWith(guessed, LotusInteraction.FatalInteraction.Create(this));
        }
        else
        {
            guesserMessage = Translations.GuessAnnouncementMessage.Formatted(MyPlayer.name);
            MyPlayer.InteractWith(MyPlayer, LotusInteraction.FatalInteraction.Create(this));
        }
    }


    [RoleAction(RoleActionType.MeetingEnd, triggerAfterDeath: true)]
    public void CheckRevive()
    {
        if (guesserMessage != null) GuesserHandler(guesserMessage).Send();
    }

    [RoleAction(RoleActionType.Chat)]
    public void DoGuesserVoting(PlayerControl player, string message, GameState state)
    {
        DevLogger.Log($"Message: {message} | Guessing player: {guessingPlayer}");
        if (state is not GameState.InMeeting) return;
        if (player.PlayerId != MyPlayer.PlayerId) return;
        if (guessingPlayer == byte.MaxValue) return;
        if (!(message.StartsWith("/role") || message.StartsWith("/r"))) return;
        string[] split = message.Replace("/role", "/r").Split(" ");
        if (split.Length == 1)
        {
            GuesserHandler(Translations.TypeRText).Send(MyPlayer);
            return;
        }

        string roleName = split[1..].Fuse(" ");
        CustomRole? role = CustomRoleManager.AllRoles.FirstOrOptional(r => string.Equals(r.RoleName, roleName, StringComparison.CurrentCultureIgnoreCase))
            .CoalesceEmpty(() => CustomRoleManager.AllRoles.FirstOrOptional(r => r.RoleName.ToLower().Contains(roleName.ToLower())))
            .CoalesceEmpty(() => CustomRoleManager.AllRoles.FirstOrOptional(r => r.EnglishRoleName.ToLower().Contains(roleName.ToLower())))
            .OrElse(null!);
        if (role == null!)
        {
            GuesserHandler(Translations.UnknownRole.Formatted(roleName)).Send(MyPlayer);
            return;
        }

        guessedRole = role;

        GuesserHandler(Translations.PickedRoleText.Formatted(Players.FindPlayerById(guessingPlayer)?.name, guessedRole.RoleName)).Send(MyPlayer);
    }

    protected ChatHandler GuesserHandler(string message) => ChatHandler.Of(message, RoleColor.Colorize(Translations.GuesserTitle)).LeftAlign();

    [Localized(nameof(Guesser))]
    private class Translations
    {
        [Localized(nameof(Guesser))]
        public static string GuesserTitle = "Guesser";

        [Localized(nameof(PickedRoleText))]
        public static string PickedRoleText = "You are about to guess {0} as {1}. If you are certain about this, vote {0} again to finalize your guess. Otherwise you can pick another player by voting a different player.. OR pick a different role by typing /r [rolename]";

        [Localized(nameof(PickedPlayerText))]
        public static string PickedPlayerText = "You are guessing {0}'s role. To guess their role type /r [rolename].";

        [Localized(nameof(TypeRText))]
        public static string TypeRText = "Please type /r [roleName] to guess that role.";

        [Localized(nameof(UnknownRole))]
        public static string UnknownRole = "Unknown role {0}. You can use /perc to view all enabled roles.";

        [Localized(nameof(FinishedGuessingText))]
        public static string FinishedGuessingText = "You have confirmed your guess. If you are not dead, you may now vote normally.";

        [Localized(nameof(ErrorCompletingGuess))]
        public static string ErrorCompletingGuess = "Error completing guess. You may try and guess again.";

        [Localized(nameof(GuessAnnouncementMessage))]
        public static string GuessAnnouncementMessage = "The guesser has made a guess. {0} died.";
    }


    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier.VanillaRole(RoleTypes.Crewmate);
}