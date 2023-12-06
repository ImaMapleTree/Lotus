using Lotus.Factions.Interfaces;
using Lotus.Extensions;
using Lotus.Roles2;
using Lotus.Roles2.Operations;

namespace Lotus.Factions;

public static class FactionExtensions
{
    public static Relation Relationship(this PlayerControl player, PlayerControl other) => RoleOperations.Current.Relationship(player, other);

    public static Relation Relationship(this PlayerControl player, UnifiedRoleDefinition other) => RoleOperations.Current.Relationship(player.PrimaryRole(), other);

    public static Relation Relationship(this PlayerControl player, IFaction faction) => RoleOperations.Current.Relationship(player.PrimaryRole(), faction);
}