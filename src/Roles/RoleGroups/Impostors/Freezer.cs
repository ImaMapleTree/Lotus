using Lotus.API;
using Lotus.GUI;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Extensions;
using Lotus.Options;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Freezer : Vanilla.Shapeshifter
{
    private PlayerControl currentFreezerTarget;
    private float freezeCooldown;
    private Cooldown freezeDuration;
    private bool canVent;

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    [RoleAction(RoleActionType.SelfReportBody)]
    [RoleAction(RoleActionType.AnyReportedBody)]
    private void OnBodyReport()
    {
        if (currentFreezerTarget != null)
            ResetSpeed();
    }

    [RoleAction(RoleActionType.MyDeath)]
    [RoleAction(RoleActionType.SelfExiled)]
    private void OnExile()
    {
        if (currentFreezerTarget != null)
            ResetSpeed();
    }

    [RoleAction(RoleActionType.Shapeshift)]
    private void OnShapeshift(PlayerControl target)
    {
        if (freezeDuration.NotReady()) return;
        freezeDuration.Start();
        GameOptionOverride[] overrides = { new(Override.PlayerSpeedMod, 0.0001f) };
        target.GetCustomRole().SyncOptions(overrides);
        currentFreezerTarget = target;
    }
    [RoleAction(RoleActionType.Unshapeshift)]
    private void OnUnshapeshift()
    {
        freezeDuration.Finish();
        ResetSpeed();
        currentFreezerTarget = null;
    }

    private void ResetSpeed()
    {
        if (currentFreezerTarget == null) return;
        GameOptionOverride[] overrides = { new(Override.PlayerSpeedMod, AUSettings.PlayerSpeedMod()) };
        currentFreezerTarget.GetCustomRole().SyncOptions(overrides);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Freeze Cooldown")
                .Bind(v => freezeCooldown = (float)v)
                .AddFloatRange(5f, 120f, 2.5f, 10, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Freeze Duration")
                .Bind(v => freezeDuration.Duration = (float)v)
                .AddFloatRange(5f, 60f, 2.5f, 4, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Can Vent")
                .Bind(v => canVent = (bool)v)
                .AddOnOffValues()
                .Build());
    protected override RoleModifier Modify(RoleModifier modifier) =>
        base.Modify(modifier)
            .CanVent(canVent)
            .OptionOverride(Override.ShapeshiftDuration, freezeDuration.Duration)
            .OptionOverride(Override.ShapeshiftCooldown, freezeCooldown);
}