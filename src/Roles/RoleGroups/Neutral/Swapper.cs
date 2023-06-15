using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Vanilla.Meetings;
using Lotus.Chat;
using Lotus.Extensions;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using static Lotus.Roles.RoleGroups.Neutral.Swapper.SwapperTranslations;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Swapper : Crewmate
{
    private byte target1 = byte.MaxValue;
    private byte target2 = byte.MaxValue;
    private bool skippedAbility;
    private int swapsPerGame;
    private int currentSwaps;

    protected override void PostSetup()
    {
        currentSwaps = swapsPerGame;
        if (swapsPerGame == -1) return;
        CounterComponent counter = new(new LiveString(() => RoleUtils.Counter(currentSwaps, swapsPerGame)), new[] { GameState.InMeeting }, ViewMode.Additive, MyPlayer);
        MyPlayer.NameModel().GetComponentHolder<CounterHolder>().Add(counter);
    }

    [RoleAction(RoleActionType.RoundEnd, triggerAfterDeath: true)]
    private void SendSwappingMessage()
    {
        target1 = byte.MaxValue;
        target2 = byte.MaxValue;
        skippedAbility = false;
        if (MyPlayer.IsAlive()) Async.Schedule(() => ChatHandler.Of(SwapperInfoMessage, RoleColor.Colorize(SwapperAbility)).LeftAlign().Send(MyPlayer), 1f);
    }

    [RoleAction(RoleActionType.MyVote)]
    private void SwapperVoteSelection(Optional<PlayerControl> votedPlayer, MeetingDelegate meetingDelegate, ActionHandle handle)
    {
        if (currentSwaps == 0) return;
        // If target2 is selected then we're voting normally
        if (target2 != byte.MaxValue || skippedAbility) return;
        // If target 1 is selected then select target 2
        if (target1 != byte.MaxValue)
        {
            votedPlayer.Handle(player =>
            {
                handle.Cancel();
                // If the new voted player is target 1 then cancel swapping target 1
                if (player.PlayerId == target1)
                {
                    CHandler().Message(SwapperUnselectMessage.Formatted(Players.FindPlayerById(target1)?.name)).Send(MyPlayer);
                    target1 = byte.MaxValue;
                    return;
                }

                currentSwaps--;
                target2 = player.PlayerId;
                CHandler().Message(SwapperSelectMessage2.Formatted(Utils.GetPlayerById(target2)?.name, Utils.GetPlayerById(target1)?.name)).Send(MyPlayer);
            }, () => meetingDelegate.CastVote(MyPlayer, Utils.PlayerById(target1)));
        }
        // Target 1 is not selected yet so this is either a complete skip or
        else
        {
            handle.Cancel();
            votedPlayer.Handle(player =>
            {
                target1 = player.PlayerId;
                CHandler().Message(SwapperSelectMessage1.Formatted(target1 == byte.MaxValue ? "No One" : player.name)).Send(MyPlayer);
            }, () => skippedAbility = true);
        }
    }

    [RoleAction(RoleActionType.VotingComplete)]
    private void SwapVotes(MeetingDelegate meetingDelegate)
    {
        if (target1 == byte.MaxValue || target2 == byte.MaxValue) return;
        VentLogger.Trace($"Swapping Votes for {Utils.GetPlayerById(target1)?.name} <=> {Utils.GetPlayerById(target2)?.name}", "Swapper");

        ChatHandler handler = ChatHandler.Of(SwapperPublicMessage.Formatted(Utils.GetPlayerById(target1)?.name, Utils.GetPlayerById(target2)?.name))
            .Title(t => t.Text(SwapperAbility).Color(RoleColor).PrefixSuffix("↔").Build());

        Async.Schedule(() => handler.Send(), 0.1f);

        List<byte> votesForPlayer1 = new();
        List<byte> votesForPlayer2 = new();
        meetingDelegate.CurrentVotes().ForEach(kv =>
        {
            // For each vote that player has cast
            kv.Value.ForEach(b =>
            {
                // If that vote exists (not-skip)
                b.IfPresent(bb =>
                {
                    // If the vote is for target1 or target2 add this player to that respective list
                    if (target1 == bb) votesForPlayer1.Add(kv.Key);
                    else if (target2 == bb) votesForPlayer2.Add(kv.Key);
                });
            });
        });

        Optional<byte> player1Optional = Optional<byte>.NonNull(target1);
        Optional<byte> player2Optional = Optional<byte>.NonNull(target2);

        votesForPlayer1.ForEach(player =>
        {
            meetingDelegate.RemoveVote(player, player1Optional);
            meetingDelegate.CastVote(player, player2Optional);
        });

        votesForPlayer2.ForEach(player =>
        {
            meetingDelegate.RemoveVote(player, player2Optional);
            meetingDelegate.CastVote(player, player1Optional);
        });

        meetingDelegate.CalculateExiledPlayer();
    }

    private ChatHandler CHandler() => Chat.ChatHandler.Of(title: RoleColor.Colorize(SwapperAbility)).LeftAlign();

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name(TranslationUtil.Colorize(SwapsPerGame, RoleColor))
                .Key("Swaps Per Game")
                .Description("The number of times the Swapper can swap per game")
                .Value(v => v.Text("∞").Color(ModConstants.Palette.InfinityColor).Value(-1).Build())
                .AddIntRange(1, 20, 1)
                .BindInt(i => swapsPerGame = i)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor("#66E666");

    [Localized("Swapper")]
    internal static class SwapperTranslations
    {
        [Localized(nameof(SwapperInfoMessage), ForceOverride = true)]
        public static string SwapperInfoMessage =
            "You are Swapper. To swap the votes received by two players, vote two separate players.\nYou may change the first swapped person by re-voting them.\nAdditionally, you may bypass this ability by initially skipping.";

        [Localized(nameof(SwapperSelectMessage1))]
        public static string SwapperSelectMessage1 = "You've selected to swap {0}'s votes.";

        [Localized(nameof(SwapperSelectMessage2))]
        public static string SwapperSelectMessage2 = "You've selected to swap {0}'s votes. {1} and {0} will now have their votes swapped.";

        [Localized(nameof(SwapperUnselectMessage))]
        public static string SwapperUnselectMessage = "You've unselected {0}.";

        [Localized(nameof(SwapperPublicMessage), ForceOverride = true)]
        public static string SwapperPublicMessage = "Swapper has swapped the votes received by {0} and {1}!";

        [Localized(nameof(SwapperAbility))]
        public static string SwapperAbility = "Swapper Ability";

        [Localized("Options.SwapsPerGame")]
        public static string SwapsPerGame = "Swaps::0 Per Game";
    }
}