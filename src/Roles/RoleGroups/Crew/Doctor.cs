using TOHTOR.API;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;

namespace TOHTOR.Roles.RoleGroups.Crew;

public class Doctor : Scientist
{
    [RoleAction(RoleActionType.AnyDeath)]
    private void DoctorAnyDeath(PlayerControl dead)
    {
        string causeOfDeath = Game.GameHistory.GetCauseOfDeath(dead.PlayerId).Map(de => de.SimpleName()).OrElse("Unknown");
        dead.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(causeOfDeath, new[] { GameState.InMeeting }, viewers: MyPlayer));
    }
}