using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;

namespace TOHTOR.Roles.RoleGroups.Crew;

public class Doctor : Scientist
{
    [RoleAction(RoleActionType.AnyDeath)]
    private void DoctorAnyDeath(PlayerControl dead)
    {
        string causeOfDeath = Game.GameHistory.GetCauseOfDeath(dead.PlayerId).Map(de => de.SimpleName()).OrElse("Unknown");
        dead.NameModel().GetComponentHolder<TextHolder>().Add(new TextComponent(causeOfDeath, new[] { GameState.InMeeting }, viewers: MyPlayer));
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.5f, 1f, 0.87f));
}