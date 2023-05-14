using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.API;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Witch: Vanilla.Impostor
{
    private bool canSwitchWithButton;

    [NewOnSetup] private Dictionary<byte, Remote<IndicatorComponent>> remotes;
    [NewOnSetup] private List<PlayerControl> cursedPlayers;
    private WitchMode mode = WitchMode.Killing;

    [UIComponent(UI.Text)]
    private string WitchModeDisplay() =>
        new Color(0.49f, 0.6f, 0.22f).Colorize("Mode: ") + (mode is WitchMode.Killing
            ? Color.red.Colorize("Kill")
            : new Color(0.63f, 0.45f, 1f).Colorize("Spell"));


    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (mode is WitchMode.Killing)
        {
            mode = WitchMode.Cursing;
            return base.TryKill(target);
        }

        mode = WitchMode.Killing;
        if (MyPlayer.InteractWith(target, DirectInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return false;

        Game.MatchData.GameHistory.AddEvent(new CursedEvent(MyPlayer, target));
        cursedPlayers.Add(target);
        remotes.GetValueOrDefault(target.PlayerId)?.Delete();
        LiveString liveString = new("†", Color.red);
        IndicatorComponent component = new(liveString, GameState.InMeeting);
        remotes[target.PlayerId] = target.NameModel().GetComponentHolder<IndicatorHolder>().Add(component);
        target.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(liveString, GameState.Roaming, viewers: MyPlayer));
        MyPlayer.RpcGuardAndKill(target);
        return true;
    }

    [RoleAction(RoleActionType.OnPet)]
    private void WitchSwitchModes() => mode = canSwitchWithButton ? mode is WitchMode.Killing ? mode = WitchMode.Cursing : WitchMode.Killing : mode;

    [RoleAction(RoleActionType.SelfExiled)]
    [RoleAction(RoleActionType.MyDeath)]
    private void ClearCursed()
    {
        remotes.Values.ForEach(v => v.Delete());
        remotes.Clear();
        cursedPlayers.Clear();
    }

    [RoleAction(RoleActionType.RoundStart)]
    private void KilledCursed()  {
        cursedPlayers.Where(p => p.IsAlive()).ForEach(p =>
        {
            FatalIntent intent = new(true, () => new CursedDeathEvent(p, MyPlayer));
            p.InteractWith(p, new IndirectInteraction(intent, this));
        });
        ClearCursed();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Can Freely Switch Modes")
                .Bind(v => canSwitchWithButton = (bool)v)
                .AddOnOffValues().Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).OptionOverride(new IndirectKillCooldown(KillCooldown, () => mode is WitchMode.Cursing));

    private enum WitchMode
    {
        Killing,
        Cursing
    }
}