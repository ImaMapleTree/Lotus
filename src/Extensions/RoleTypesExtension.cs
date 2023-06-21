using AmongUs.GameOptions;

namespace Lotus.Extensions;

public static class RoleTypesExtension
{
    public static bool IsImpostor(this RoleTypes roleTypes) => roleTypes is RoleTypes.Impostor or RoleTypes.Shapeshifter or RoleTypes.ImpostorGhost;
    public static bool IsCrewmate(this RoleTypes roleTypes) => roleTypes is not (RoleTypes.Impostor or RoleTypes.Shapeshifter or RoleTypes.ImpostorGhost);
}