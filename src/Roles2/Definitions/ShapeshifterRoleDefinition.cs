using AmongUs.GameOptions;
using Lotus.API;
using Lotus.Roles.Overrides;

namespace Lotus.Roles2.Definitions;

public class ShapeshifterRoleDefinition: ImpostorRoleDefinition
{
    public virtual float ShapeshiftCooldown { get => shapeshiftCooldown <= -1 ? AUSettings.ShapeshifterCooldown() : shapeshiftCooldown; set => shapeshiftCooldown = value; }
    private float shapeshiftCooldown = -1;

    public virtual float ShapeshiftDuration { get => shapeshiftDuration <= -1 ? AUSettings.ShapeshifterDuration() : shapeshiftDuration; set => shapeshiftDuration = value; }
    private float shapeshiftDuration = -1;

    public override RoleTypes Role => RoleTypes.Shapeshifter;

    public ShapeshifterRoleDefinition()
    {
        AddGameOptionOverride(new GameOptionOverride(Override.ShapeshiftCooldown, () => ShapeshiftCooldown));
        AddGameOptionOverride(new GameOptionOverride(Override.ShapeshiftDuration, () => ShapeshiftDuration));
    }
}