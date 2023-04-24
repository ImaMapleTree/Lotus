using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Factions.Impostors;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.Crew;

public class Psychic: Crewmate
{
    private int numberOfPlayers;
    private bool nonImpostorAreEvil;

    [NewOnSetup]
    private List<Remote<NameComponent>> remotes;


    [RoleAction(RoleActionType.RoundEnd)]
    private void MarkMeetingPlayers()
    {
        bool IsEvil(PlayerControl player) => nonImpostorAreEvil
            ? MyPlayer.Relationship(player) is not Relation.None
            : player.GetCustomRole().Faction is ImpostorFaction;

        List<PlayerControl> eligiblePlayers = Game.GetAlivePlayers().Where(IsEvil).ToList();
        if (eligiblePlayers.Count == 0) return;

        PlayerControl evilPlayer = eligiblePlayers.GetRandom();
        List<PlayerControl> targetPlayers = new() { evilPlayer };
        List<PlayerControl> remainingPlayers = Game.GetAlivePlayers().Where(p => p.PlayerId != MyPlayer.PlayerId && p.PlayerId != evilPlayer.PlayerId).ToList();

        while (targetPlayers.Count < numberOfPlayers && remainingPlayers.Count != 0) targetPlayers.Add(remainingPlayers.PopRandom());

        targetPlayers.ForEach(p =>
            remotes.Add(p.NameModel().GetComponentHolder<NameHolder>().Add(new ColoredNameComponent(p, Color.red, GameState.InMeeting, viewers: MyPlayer))));
    }

    [RoleAction(RoleActionType.RoundStart)]
    private void CleanupMarkedPlayers()
    {
        remotes.ForEach(r => r.Delete());
        remotes.Clear();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Highlighted Players")
                .AddIntRange(1, 14, 1, 2)
                .BindInt(i => numberOfPlayers = i)
                .Build())
            .SubOption(sub => sub.Name("Non-impostor Killing Are Evil")
                .AddOnOffValues()
                .BindBool(b => nonImpostorAreEvil = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.44f, 0.41f, 0.55f));
}