using Lotus.API.Odyssey;
using Lotus.Roles.Events;
using Lotus.Roles.RoleGroups.Undead.Roles;
using Lotus.API;
using VentLib.Utilities;

namespace Lotus.Roles.RoleGroups.Undead.Events;

public class ConvertEvent : TargetedAbilityEvent
{
    public ConvertEvent(PlayerControl source, PlayerControl target, bool successful = true) : base(source, target, successful)
    {
    }

    public override string Message() => $"{UndeadRole.UndeadColor.Colorize(Game.GetName(Player()))} turned {ModConstants.HColor2.Colorize(Game.GetName(Target()))} to the Undead.";
}