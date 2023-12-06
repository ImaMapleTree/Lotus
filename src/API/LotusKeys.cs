using Lotus.Roles.Internals.Enums;

namespace Lotus.API;

public class LotusKeys
{
    public static NamespacedKey<SpecialType> AuxiliaryRoleType = NamespacedKey.Lotus<SpecialType>(nameof(AuxiliaryRoleType));
    public static NamespacedKey<string> ModifierSymbol = NamespacedKey.Lotus<string>(nameof(ModifierSymbol));
    public static NamespacedKey<bool> GloballyManagedRole = NamespacedKey.Lotus<bool>(nameof(GloballyManagedRole));
}