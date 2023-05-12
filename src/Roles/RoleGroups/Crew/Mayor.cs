using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.API.Vanilla.Meetings;
using TOHTOR.Extensions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Patches.Systems;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Roles.RoleGroups.Crew;

[Localized("Roles.Mayor")]
public class Mayor: Crewmate
{
    private bool hasPocketMeeting;

    private int additionalVotes;

    private int totalVotes;
    private int remainingVotes;

    private bool revealToVote;
    private bool revealed;

    private FixedUpdateLock updateLock = new(0.25f);
    
    [Localized("RevealMessage")]
    private static string _mayorRevealMessage = "Mr. Mayor, you must reveal yourself to gain additional votes. Currently you can vote normally, but if you vote yourself you'll reveal your role to everyone and gain more votes!";

    [UIComponent(UI.Counter)]
    private string PocketCounter() => RoleUtils.Counter(remainingVotes, totalVotes);

    // Removes meeting use counter component if the option is disabled
    protected override void PostSetup()
    {
        remainingVotes = totalVotes;
        if (!hasPocketMeeting) MyPlayer.NameModel().GetComponentHolder<CounterHolder>().RemoveAt(1);
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
            Utils.SendMessage($"{MyPlayer.name} revealed themself as Mayor!", title: "Mayor Reveal");
            List<PlayerControl> allPlayers = Game.GetAllPlayers().ToList();
            MyPlayer.NameModel().GetComponentHolder<RoleHolder>()[0].SetViewerSupplier(() => allPlayers);
            return;
        }
        if (!voted.Exists()) return;
        for (int i = 0; i < additionalVotes; i++) meetingDelegate.AddVote(MyPlayer, voted);
    }

    [RoleAction(RoleActionType.RoundEnd)]
    private void MayorNotify()
    {
       if (revealToVote && !revealed) Utils.SendMessage(_mayorRevealMessage, MyPlayer.PlayerId);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Reveal for Votes")
                .AddOnOffValues(false)
                .BindBool(b => revealToVote = b)
                .Build())
            .SubOption(sub => sub.Name("Mayor Additional Votes")
                .AddIntRange(0, 10, 1, 1)
                .BindInt(i => additionalVotes = i)
                .Build())
            .SubOption(sub => sub.Name("Pocket Meeting")
                .AddOnOffValues()
                .BindBool(b => hasPocketMeeting = b)
                .ShowSubOptionPredicate(o => (bool)o)
                .SubOption(sub2 => sub2.Name("Number of Uses")
                    .AddIntRange(1, 20, 1, 2)
                    .BindInt(i => totalVotes = i)
                    .Build())
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.13f, 0.3f, 0.26f));
}