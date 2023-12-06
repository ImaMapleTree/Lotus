using Lotus.Roles2;

namespace Lotus.Roles.Interactions.Interfaces;

// ReSharper disable once InconsistentNaming
public interface Interaction
{
    public UnifiedRoleDefinition Emitter();

    public Intent Intent { get; protected set; }
    /// <summary>
    /// If this interaction is impossible to cancel (aka promised)
    /// </summary>
    public bool IsPromised { get; protected set; }

    public Interaction Modify(Intent intent);
}