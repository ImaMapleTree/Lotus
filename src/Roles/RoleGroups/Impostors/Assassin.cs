using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Subroles;
using UnityEngine;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Assassin: Guesser, ISabotagerRole
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
        base.Modify(roleModifier)
            .RoleColor(Color.red)
            .Faction(FactionInstances.Impostors)
            .RoleAbilityFlags(RoleAbilityFlag.IsAbleToKill)
            .VanillaRole(RoleTypes.Impostor);

    private static class Translations
    {
        public static class Options
        {
        }
    }
}