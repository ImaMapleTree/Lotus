namespace Lotus.Server.Modifiers;

public interface IPatchModifier
{
    public IServerPatch Modify(IServerPatch initialPatch);

    public PatchModifierPriority Priority() => PatchModifierPriority.Normal;
}