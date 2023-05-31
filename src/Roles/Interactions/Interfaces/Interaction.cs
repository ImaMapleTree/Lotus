namespace Lotus.Roles.Interactions.Interfaces;

// ReSharper disable once InconsistentNaming
public interface Interaction
{
    public CustomRole Emitter();

    public Intent Intent();

    public Interaction Modify(Intent intent);
}