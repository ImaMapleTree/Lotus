using System.Collections.Generic;
using System.Linq;
using Lotus.API.Player;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.Extensions;
using Lotus.Options;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Vampiress : Impostor
{
    private float killDelay;
    private VampireMode mode = VampireMode.Biting;
    [NewOnSetup] private HashSet<byte> bitten = null!;

    [UIComponent(UI.Text)]
    private string CurrentMode() => mode is VampireMode.Biting ? RoleColor.Colorize("(Bite)") : RoleColor.Colorize("(Kill)");

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        SyncOptions();
        if (mode is VampireMode.Killing) return base.TryKill(target);
        MyPlayer.RpcMark(target);
        InteractionResult result = MyPlayer.InteractWith(target, LotusInteraction.HostileInteraction.Create(this));
        if (result is InteractionResult.Halt) return false;

        bitten.Add(target.PlayerId);
        Async.Schedule(() =>
        {
            FatalIntent intent = new(true, () => new BittenDeathEvent(target, MyPlayer));
            DelayedInteraction interaction = new(intent, killDelay, this);
            MyPlayer.InteractWith(target, interaction);
        }, killDelay);

        return false;
    }

    [RoleAction(RoleActionType.RoundStart)]
    private void ResetKillState()
    {
        mode = VampireMode.Killing;
    }

    [RoleAction(RoleActionType.OnPet)]
    public void SwitchMode()
    {
        VampireMode currentMode = mode;
        mode = mode is VampireMode.Killing ? VampireMode.Biting : VampireMode.Killing;
        VentLogger.Trace($"Swapping Vampire Mode: {currentMode} => {mode}");
    }

    [RoleAction(RoleActionType.MeetingCalled, triggerAfterDeath: true)]
    public void KillBitten()
    {
        bitten.Filter(Players.PlayerById).Where(p => p.IsAlive()).ForEach(p =>
        {
            FatalIntent intent = new(true, () => new BittenDeathEvent(p, MyPlayer));
            DelayedInteraction interaction = new(intent, killDelay, this);
            MyPlayer.InteractWith(p, interaction);
        });
        bitten.Clear();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream))
            .SubOption(sub => sub
                .Name("Kill Delay")
                .BindFloat(v => killDelay = v)
                .AddFloatRange(2.5f, 60f, 2.5f, 2, GeneralOptionTranslations.SecondsSuffix)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(new IndirectKillCooldown(KillCooldown, () => mode is VampireMode.Biting))
            .RoleFlags(RoleFlag.VariationRole);

    public enum VampireMode
    {
        Killing,
        Biting
    }
}