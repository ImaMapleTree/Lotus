using Lotus.Factions.Interfaces;
using Lotus.Roles;
using Lotus.Extensions;

namespace Lotus.Factions;

public static class FactionExtensions
{
    public static Relation Relationship(this PlayerControl player, PlayerControl other) => player.GetCustomRole().Relationship(other);

    public static Relation Relationship(this PlayerControl player, CustomRole other) => player.Relationship(other.Faction);

    public static Relation Relationship(this PlayerControl player, IFaction faction) => player.GetCustomRole().Relationship(faction);

    public static Relation Relationship(this CustomRole role, CustomRole other) => role.Relationship(other.Faction);

    public static Relation Relationship(this CustomRole role, IFaction faction) => role.Faction.Relationship(faction);
}