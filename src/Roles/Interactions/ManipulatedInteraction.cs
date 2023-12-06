using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles2;

namespace Lotus.Roles.Interactions;

public class ManipulatedInteraction : LotusInteraction, IManipulatedInteraction
{
    private PlayerControl manipulator;

    public ManipulatedInteraction(Intent intent, UnifiedRoleDefinition victim, PlayerControl manipulator) : base(intent, victim)
    {
        this.manipulator = manipulator;
    }

    public PlayerControl Manipulator() => manipulator;

    public override Interaction Modify(Intent intent) => new ManipulatedInteraction(intent, Emitter(), manipulator);
}