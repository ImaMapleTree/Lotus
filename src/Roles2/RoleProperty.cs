using System;
using System.Reflection;
using Lotus.Extensions;

namespace Lotus.Roles2;

public class RoleProperty
{
    public static RoleProperty IsModifier = new(nameof(IsModifier));
    public static RoleProperty IsAbleToKill = new(nameof(IsAbleToKill));
    public static RoleProperty CannotWinAlone = new(nameof(CannotWinAlone));
    public static RoleProperty IsApparition = new(nameof(IsApparition));

    public string Name { get; }
    public ulong CollisionNumber { get; }

    public RoleProperty(string name, ulong collisionNumber = 0)
    {
        Name = name;

        Assembly callingAssembly;
        if (collisionNumber == 0 && (callingAssembly = Assembly.GetCallingAssembly()) != typeof(ProjectLotus).Assembly)
            collisionNumber = callingAssembly.SemiConsistentHash();

        CollisionNumber = collisionNumber;
    }

    public override bool Equals(object? obj) => obj is RoleProperty roleProperty && roleProperty.Name == Name && roleProperty.CollisionNumber == CollisionNumber;
    public override int GetHashCode() => HashCode.Combine(Name.GetHashCode(), CollisionNumber.GetHashCode());
}