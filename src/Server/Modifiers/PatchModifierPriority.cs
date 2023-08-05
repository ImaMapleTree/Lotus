namespace Lotus.Server.Modifiers;

public struct PatchModifierPriority
{
    internal static readonly PatchModifierPriority AbsoluteFirst = new(uint.MaxValue);
    public static readonly PatchModifierPriority First = new(1000);
    public static readonly PatchModifierPriority High = new(750);
    public static readonly PatchModifierPriority Normal = new(500);
    public static readonly PatchModifierPriority Low = new(250);
    public static readonly PatchModifierPriority Last = new(1);
    internal static readonly PatchModifierPriority AbsoluteLast = new(0);

    internal readonly uint Value;

    private PatchModifierPriority(uint value)
    {
        this.Value = value;
    }
}