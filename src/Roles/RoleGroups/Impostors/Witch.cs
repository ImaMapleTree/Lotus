using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Roles.Events;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;

namespace TOHTOR.Roles.RoleGroups.Impostors;

public class Witch: Vanilla.Impostor
{
    private bool canSwitchWithButton;

    private Dictionary<byte, Remote<IndicatorComponent>> remotes;
    private List<PlayerControl> cursedPlayers;
    private WitchMode mode = WitchMode.Killing;

    protected override void Setup(PlayerControl player) => cursedPlayers = new List<PlayerControl>();
    protected override void PostSetup() => remotes = new Dictionary<byte, Remote<IndicatorComponent>>();

    [DynElement(UI.Misc)]
    private string WitchModeDisplay() =>
        new Color(0.49f, 0.6f, 0.22f).Colorize("Mode: ") + (mode is WitchMode.Killing
            ? Color.red.Colorize("Kill")
            : new Color(0.63f, 0.45f, 1f).Colorize("Spell"));


    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        SyncOptions();
        if (mode is WitchMode.Killing)
        {
            mode = WitchMode.Cursing;
            return base.TryKill(target);
        }

        mode = WitchMode.Killing;
        InteractionResult result = CheckInteractions(target.GetCustomRole(), target);
        if (result is InteractionResult.Halt) return false;

        Game.GameHistory.AddEvent(new CursedEvent(MyPlayer, target));
        cursedPlayers.Add(target);
        remotes.GetValueOrDefault(target.PlayerId)?.Delete();
        IndicatorComponent component = new(new LiveString("†", Color.red), GameState.InMeeting);
        remotes[target.PlayerId] = target.NameModel().GetComponentHolder<IndicatorHolder>().Add(component);

        MyPlayer.RpcGuardAndKill(target);
        return true;
    }

    [RoleAction(RoleActionType.OtherExiled)]
    private void WitchKillCheck()
    {
        cursedPlayers.Where(p => !p.Data.IsDead).Do(p =>
        {
            p.Attack(p, () => new CursedDeathEvent(p, MyPlayer));
            remotes.GetValueOrDefault(p.PlayerId)?.Delete();
        });
        cursedPlayers.Clear();
    }

    [RoleAction(RoleActionType.OnPet)]
    private void WitchSwitchModes() => mode = canSwitchWithButton ? mode is WitchMode.Killing ? mode = WitchMode.Cursing : WitchMode.Killing : mode;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Can Freely Switch Modes")
                .Bind(v => canSwitchWithButton = (bool)v)
                .AddOnOffValues().Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(Override.KillCooldown, KillCooldown * 2, () => mode == WitchMode.Cursing);

    private enum WitchMode
    {
        Killing,
        Cursing
    }
}