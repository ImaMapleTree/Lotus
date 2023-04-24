using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.Roles.Events;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using static TOHTOR.Roles.RoleGroups.Crew.Escort;

namespace TOHTOR.Roles.RoleGroups.Impostors;

public class Consort: Impostor
{
    private float roleblockDuration;
    private bool blocking;

    [NewOnSetup] private Dictionary<byte, BlockDelegate> blockedPlayers;

    [UIComponent(UI.Cooldown)]
    private Cooldown roleblockCooldown;

    [UIComponent(UI.Text)]
    private string BlockingText() => !blocking ? "" : Color.red.Colorize("Blocking");

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (!blocking) return base.TryKill(target);
        if (blockedPlayers.ContainsKey(target.PlayerId)) return false;

        if (MyPlayer.InteractWith(target, DirectInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return false;

        roleblockCooldown.Start();
        blocking = false;

        blockedPlayers[target.PlayerId] = BlockDelegate.Block(target, MyPlayer, roleblockDuration);
        MyPlayer.RpcGuardAndKill(target);
        Game.GameHistory.AddEvent(new GenericTargetedEvent(MyPlayer, target, $"{RoleColor.Colorize(MyPlayer.UnalteredName())} role blocked {target.GetRoleColor().Colorize(target.UnalteredName())}."));

        if (roleblockDuration > 0) Async.Schedule(() => blockedPlayers.Remove(target.PlayerId), roleblockDuration);
        return false;
    }

    [RoleAction(RoleActionType.OnPet)]
    private void ChangeToBlockMode()
    {
        if (roleblockCooldown.IsReady()) blocking = !blocking;
    }

    [RoleAction(RoleActionType.RoundStart)]
    [RoleAction(RoleActionType.RoundEnd)]
    private void UnblockPlayers()
    {
        blockedPlayers.ToArray().ForEach(k =>
        {
            blockedPlayers.Remove(k.Key);
            k.Value.Delete();
        });
    }

    [RoleAction(RoleActionType.AnyPlayerAction)]
    private void BlockAction(PlayerControl source, ActionHandle handle, RoleAction action)
    {
        if (action.Blockable) Block(source, handle);
    }

    [RoleAction(RoleActionType.AnyEnterVent)]
    private void Block(PlayerControl source, ActionHandle handle)
    {
        BlockDelegate? blockDelegate = blockedPlayers.GetValueOrDefault(source.PlayerId);
        if (blockDelegate == null) return;

        handle.Cancel();
        blockDelegate.UpdateDelegate();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Roleblock Cooldown")
                .BindFloat(roleblockCooldown.SetDuration)
                .AddFloatRange(0, 120, 2.5f, 18, "s")
                .Build())
            .SubOption(sub => sub
                .Name("Roleblock Duration")
                .BindFloat(v => roleblockDuration = v)
                .Value(v => v.Text("Until Meeting").Value(-1f).Build())
                .AddFloatRange(5, 120, 5, suffix: "s")
                .Build());


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(Override.KillCooldown, KillCooldown * 2, () => blocking);
}