using TOHTOR.Extensions;
using TOHTOR.Factions.Interfaces;
using TOHTOR.Roles;

namespace TOHTOR.Factions;

public static class FactionExtensions
{
    public static Relation Relationship(this PlayerControl player, PlayerControl other) => player.Relationship(other.GetCustomRole());

    public static Relation Relationship(this PlayerControl player, CustomRole other) => player.Relationship(other.Faction);

    public static Relation Relationship(this PlayerControl player, IFaction faction) => player.GetCustomRole().Relationship(faction);

    public static Relation Relationship(this CustomRole role, CustomRole other) => role.Relationship(other.Faction);

    public static Relation Relationship(this CustomRole role, IFaction faction) => role.Faction.Relationship(faction);
}