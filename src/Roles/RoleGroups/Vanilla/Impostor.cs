using AmongUs.GameOptions;
using TOHTOR.API;
using TOHTOR.Factions;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.Internals.Interfaces;
using UnityEngine;

namespace TOHTOR.Roles.RoleGroups.Vanilla;

public partial class Impostor : CustomRole, IModdable
{
    public virtual bool CanSabotage() => canSabotage;
    public virtual bool CanKill() => canKill;
    protected bool canSabotage = true;
    protected bool canKill = true;
    public float KillCooldown
    {
        set => _killCooldown = value;
        get => _killCooldown ?? OriginalOptions.KillCooldown();
    }
    private float? _killCooldown;

    [RoleAction(RoleActionType.Attack, Subclassing = false)]
    public virtual bool TryKill(PlayerControl target)
    {
        SyncOptions();
        InteractionResult result = MyPlayer.InteractWith(target, SimpleInteraction.FatalInteraction.Create(this));
        Game.GameHistory.AddEvent(new KillEvent(MyPlayer, target, result is InteractionResult.Proceed));
        return result is InteractionResult.Proceed;
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier
            .VanillaRole(RoleTypes.Impostor)
            .Faction(FactionInstances.Impostors)
            .CanVent(true)
            .OptionOverride(Override.KillCooldown, KillCooldown)
            .RoleColor(Color.red);

}