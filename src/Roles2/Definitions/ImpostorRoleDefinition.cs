using AmongUs.GameOptions;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Managers.History.Events;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Options.Game.Interfaces;

namespace Lotus.Roles2.Definitions;

// ReSharper disable once InconsistentNaming
public class ImpostorRoleDefinition: RoleDefinition
{
    public static readonly ImpostorRoleDefinition Defaults = new();
    public override Color RoleColor { get; set; } = Color.red;
    public override RoleTypes Role => RoleTypes.Impostor;
    public override IFaction Faction { get; set; } = FactionInstances.Impostors;
    public override IGameOptionTab OptionTab => DefaultTabs.ImpostorsTab;

    public virtual float KillCooldown { get => killCooldown <= -1 ? AUSettings.KillCooldown() : killCooldown; set => killCooldown = value; }
    private float killCooldown = -1;

    public virtual int KillDistance { get => killDistance <= -1 ? AUSettings.KillDistance() : killDistance; set => killDistance = value; }
    private int killDistance = -1;

    public virtual bool CanVent { get; set; } = true;
    public virtual bool CanSabotage { get; set; } = true;

    internal override bool CanVentInternal => CanVent;
    internal override bool CanSabotageInternal => CanSabotage;

    public ImpostorRoleDefinition()
    {
        AddGameOptionOverride(Override.KillCooldown, () => KillCooldown);
        AddGameOptionOverride(Override.KillDistance, () => KillDistance);
    }

    [RoleAction(LotusActionType.Attack)]
    public virtual bool TryKill(PlayerControl target)
    {
        InteractionResult interactionResult = this.MyPlayer.InteractWith(target, LotusInteraction.FatalInteraction.Create(this));
        Game.MatchData.GameHistory.AddEvent(new KillEvent(this.MyPlayer, target, interactionResult == InteractionResult.Proceed));
        return interactionResult == InteractionResult.Proceed;
    }
}