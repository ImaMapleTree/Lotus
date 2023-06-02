using Lotus.Roles.Interactions.Interfaces;

namespace Lotus.Roles.Interactions;

public class ManipulatedInteraction : DirectInteraction, IManipulatedInteraction
{
    private PlayerControl manipulator;

    public ManipulatedInteraction(Intent intent, CustomRole victim, PlayerControl manipulator) : base(intent, victim)
    {
        this.manipulator = manipulator;
    }

    public PlayerControl Manipulator() => manipulator;

    public override Interaction Modify(Intent intent) => new ManipulatedInteraction(intent, Emitter(), manipulator);
}