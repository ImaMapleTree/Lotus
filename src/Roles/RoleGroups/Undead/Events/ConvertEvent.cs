using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Roles.Events;
using TOHTOR.Roles.RoleGroups.Undead.Roles;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.Undead.Events;

public class ConvertEvent : TargetedAbilityEvent
{
    public ConvertEvent(PlayerControl source, PlayerControl target, bool successful = true) : base(source, target, successful)
    {
    }

    public override string Message() => $"{UndeadRole.UndeadColor.Colorize(Game.GetName(Player()))} turned {ModConstants.HColor2.Colorize(Game.GetName(Target()))} to the Undead.";
}