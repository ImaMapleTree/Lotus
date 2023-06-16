using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Vanilla.Meetings;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Holders;
using Lotus.Patches.Systems;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.Chat;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using static Lotus.Roles.RoleGroups.Crew.Mayor.Translations;

namespace Lotus.Roles.RoleGroups.Crew;

public class Mayor: Crewmate
{
    private bool hasPocketMeeting;

    private int additionalVotes;

    private int totalVotes;
    private int remainingVotes;

    private bool revealToVote;
    private bool revealed;

    private FixedUpdateLock updateLock = new(0.25f);

    [UIComponent(UI.Counter)]
    private string PocketCounter() => RoleUtils.Counter(remainingVotes, totalVotes);

    // Removes meeting use counter component if the option is disabled
    protected override void PostSetup()
    {
        remainingVotes = totalVotes;
        if (!hasPocketMeeting) MyPlayer.NameModel().GetComponentHolder<CounterHolder>().RemoveLast();
    }

    [RoleAction(RoleActionType.OnPet)]
    private void MayorPocketMeeting()
    {
        if (!updateLock.AcquireLock()) return;
        if (SabotagePatch.CurrentSabotage != null) return;
        if (!hasPocketMeeting || remainingVotes <= 0) return;
        remainingVotes--;
        MyPlayer.CmdReportDeadBody(null);
        MeetingApi.StartMeeting(creator => creator.QuickCall(MyPlayer));
    }

    [RoleAction(RoleActionType.MyVote)]
    private void MayorVotes(Optional<PlayerControl> voted, MeetingDelegate meetingDelegate, ActionHandle handle)
    {
        if (revealToVote && !revealed)
        {
            if (!voted.Map(p => p.PlayerId == MyPlayer.PlayerId).OrElse(false)) return;
            handle.Cancel();
            revealed = true;
            ChatHandler.Of(MayorRevealMessage.Formatted(MyPlayer.name)).Title(t => t.Color(RoleColor).Text(MayorRevealTitle).Build()).Send();
            List<PlayerControl> allPlayers = Game.GetAllPlayers().ToList();
            MyPlayer.NameModel().GetComponentHolder<RoleHolder>().Last().SetViewerSupplier(() => allPlayers);
            return;
        }
        if (!voted.Exists()) return;
        for (int i = 0; i < additionalVotes; i++) meetingDelegate.CastVote(MyPlayer, voted);
    }

    [RoleAction(RoleActionType.RoundEnd)]
    private void MayorNotify()
    {
       if (revealToVote && !revealed)
           ChatHandler.Of(RevealMessage).Title(t => t.Color(RoleColor).Text(MayorRevealTitle).Build()).Send(MyPlayer);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Reveal for Votes", Translations.Options.MayorReveal)
                .AddOnOffValues(false)
                .BindBool(b => revealToVote = b)
                .Build())
            .SubOption(sub => sub.KeyName("Mayor Additional Votes", Translations.Options.MayorAdditionalVotes)
                .AddIntRange(0, 10, 1, 1)
                .BindInt(i => additionalVotes = i)
                .Build())
            .SubOption(sub => sub.KeyName("Pocket Meeting", Translations.Options.PocketMeeting)
                .AddOnOffValues()
                .BindBool(b => hasPocketMeeting = b)
                .ShowSubOptionPredicate(o => (bool)o)
                .SubOption(sub2 => sub2.KeyName("Number of Uses", Translations.Options.NumberOfUses)
                    .AddIntRange(1, 20, 1, 2)
                    .BindInt(i => totalVotes = i)
                    .Build())
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.13f, 0.3f, 0.26f));

    [Localized(nameof(Mayor))]
    internal static class Translations
    {
        [Localized(nameof(RevealMessage))]
        internal static string RevealMessage = "Mr. Mayor, you must reveal yourself to gain additional votes. Currently you can vote normally, but if you vote yourself you'll reveal your role to everyone and gain more votes!";

        [Localized(nameof(MayorRevealTitle))]
        public static string MayorRevealTitle = "Mayor Reveal";

        [Localized(nameof(MayorRevealMessage))]
        public static string MayorRevealMessage = "{0} revealed themself as mayor!";

        [Localized(ModConstants.Options)]
        internal static class Options
        {
            [Localized(nameof(MayorReveal))]
            public static string MayorReveal = "Reveal for Votes";

            [Localized(nameof(MayorAdditionalVotes))]
            public static string MayorAdditionalVotes = "Mayor Additional Votes";

            [Localized(nameof(PocketMeeting))]
            public static string PocketMeeting = "Pocket Meeting";

            [Localized(nameof(NumberOfUses))]
            public static string NumberOfUses = "Number of Uses";
        }
    }
}