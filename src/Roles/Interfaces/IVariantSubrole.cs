/*using System.Diagnostics.CodeAnalysis;
using Lotus.Roles.Builtins.Base;

namespace Lotus.Roles.Interfaces;

public interface IVariantSubrole
{
    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    public static Subrole PickAssignedRole(Subrole baseRole)
    {
        if (baseRole is not IVariantSubrole variableRole) return baseRole;
        return variableRole.AssignVariation() ? variableRole.Variation() : baseRole;
    }

    /// <summary>
    /// The variation role to assign, this can change per requirements
    /// </summary>
    /// <returns>The role that will be assigned if AssignVariation returns true</returns>
    public Subrole Variation();

    /// <summary>
    /// Whether the game SHOULD current assign the variation, this is where your random number generator should go
    /// </summary>
    /// <returns>true if the variation will be assigned, otherwise false</returns>
    public bool AssignVariation();
}*/