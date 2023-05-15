using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;

namespace Lotus.Roles.RoleGroups.Impostors;

public class YinYanger : Vanilla.Impostor
{
    [NewOnSetup] private Dictionary<byte, Remote<IndicatorComponent>> remotes;

    private PlayerControl? yinPlayer;
    private PlayerControl? yangPlayer;

    private bool resetToYingYang;

    private FixedUpdateLock fixedUpdateLock = new();
    
    private bool InYinMode => yinPlayer == null || yangPlayer == null;

    [UIComponent(UI.Text)]
    private string ModeIndicator() => !InYinMode ? "" : Color.black.Colorize("Yin") + Color.white.Colorize("Yanging");

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (MyPlayer.InteractWith(target, DirectInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return false;
        if (!InYinMode) return true;
        if (yinPlayer != null && yinPlayer.PlayerId == target.PlayerId || yangPlayer != null && yangPlayer.PlayerId == target.PlayerId) return false;

        Color indicatorColor = yinPlayer == null ? Color.white : Color.black;
        IndicatorComponent component = new(new LiveString("☯", indicatorColor), GameState.Roaming, viewers: MyPlayer);
        
        remotes.GetValueOrDefault(target.PlayerId)?.Delete();
        remotes[target.PlayerId] = target.NameModel().GetComponentHolder<IndicatorHolder>().Add(component);

        if (yinPlayer == null) yinPlayer = target;
        else yangPlayer = target;
        
        SyncOptions();
        MyPlayer.RpcGuardAndKill(target);
        return true;
    }
    

    [RoleAction(RoleActionType.RoundEnd)]
    private void RoundEnd()
    {
        if (yinPlayer != null) remotes.GetValueOrDefault(yinPlayer.PlayerId)?.Delete();
        if (yangPlayer != null) remotes.GetValueOrDefault(yangPlayer.PlayerId)?.Delete();
        yinPlayer = null;
        yangPlayer = null;
    }

    [RoleAction(RoleActionType.FixedUpdate)]
    private void YingYangerKillCheck()
    {
        if (!fixedUpdateLock.AcquireLock()) return;
        if (yinPlayer == null || yangPlayer == null) return;

        if (yinPlayer.GetPlayersInAbilityRangeSorted().All(p => p.PlayerId != yangPlayer.PlayerId)) return;
        yinPlayer.InteractWith(yangPlayer, new ManipulatedInteraction(new FatalIntent(), yinPlayer.GetCustomRole(), MyPlayer));
        yangPlayer.InteractWith(yinPlayer, new ManipulatedInteraction(new FatalIntent(), yangPlayer.GetCustomRole(), MyPlayer));

        remotes.GetValueOrDefault(yinPlayer.PlayerId)?.Delete();
        remotes.GetValueOrDefault(yangPlayer.PlayerId)?.Delete();

        yinPlayer = null;
        yangPlayer = null;
    }
    
    [RoleAction(RoleActionType.Disconnect)]
    [RoleAction(RoleActionType.AnyDeath)]
    private void CheckPlayerDeaths(PlayerControl player)
    {
        if (yinPlayer != null && yinPlayer.PlayerId == player.PlayerId)
        {
            remotes.GetValueOrDefault(yinPlayer.PlayerId)?.Delete();
            yinPlayer = null;
        }
        else if (yangPlayer != null && yangPlayer.PlayerId == player.PlayerId)
        {
            remotes.GetValueOrDefault(yangPlayer.PlayerId)?.Delete();
            yinPlayer = null;
        }
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream),
            Localizer.Translate($"Roles.{nameof(YinYanger)}.YinYangCooldown", "Yin Yang Cooldown"),
            "Yin Yang Cooldown");

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(new IndirectKillCooldown(KillCooldown, () => InYinMode));
}