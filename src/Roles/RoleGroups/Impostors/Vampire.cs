using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Roles.Events;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.Overrides;
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles.RoleGroups.Impostors;

public class Vampire : Impostor, IVariableRole
{
    private static Vampiress _vampiress = new();

    private float killDelay;
    [NewOnSetup] private HashSet<byte> bitten = null!;

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        InteractionResult result = MyPlayer.InteractWith(target, DirectInteraction.HostileInteraction.Create(this));
        if (result is InteractionResult.Halt) return false;

        MyPlayer.RpcGuardAndKill(target);
        bitten.Add(target.PlayerId);
        Game.MatchData.GameHistory.AddEvent(new BittenEvent(MyPlayer, target));

        Async.Schedule(() =>
        {
            MyPlayer.InteractWith(target, CreateInteraction(target));
            bitten.Remove(target.PlayerId);
        }, killDelay);

        return false;
    }

    [RoleAction(RoleActionType.RoundStart)]
    public void ResetBitten() => bitten.Clear();

    [RoleAction(RoleActionType.RoundEnd)]
    public void KillBitten() => bitten.Filter(b => Utils.PlayerById(b)).ForEach(p => MyPlayer.InteractWith(p, CreateInteraction(p)));

    private DelayedInteraction CreateInteraction(PlayerControl target)
    {
        FatalIntent intent = new(true, () => new BittenDeathEvent(target, MyPlayer));
        return new DelayedInteraction(intent, killDelay, this);
    }

    public CustomRole Variation() => _vampiress;

    public bool AssignVariation() => Random.RandomRange(0, 100) <= _vampiress.Chance;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Kill Delay")
                .Bind(v => killDelay = (float)v)
                .AddFloatRange(2.5f, 60f, 2.5f, 2, "s")
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(new IndirectKillCooldown(KillCooldown))
            .LinkedRoles(_vampiress);

    /*case Vampire:
                    __instance.KillButton.OverrideText($"{GetString("VampireBiteButtonText")}");
                    break;*/
}