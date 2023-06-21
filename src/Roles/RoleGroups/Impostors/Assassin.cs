using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.RoleGroups.Stock;
using UnityEngine;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Assassin: GuesserRoleBase, ISabotagerRole
{
    public bool CanSabotage() => true;

    [RoleAction(RoleActionType.Attack)]
    public virtual bool TryKill(PlayerControl target)
    {
        InteractionResult result = MyPlayer.InteractWith(target, LotusInteraction.FatalInteraction.Create(this));
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, target, result is InteractionResult.Proceed));
        return result is InteractionResult.Proceed;
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier
            .RoleColor(Color.red)
            .Faction(FactionInstances.Impostors)
            .RoleAbilityFlags(RoleAbilityFlag.IsAbleToKill)
            .VanillaRole(RoleTypes.Impostor);
}