using Lotus.Factions;
using Lotus.Factions.Neutrals;

namespace Lotus.Roles.Subroles.Romantics;

public class RomanticFaction: Solo
{
    public byte Partner = byte.MaxValue;

    public override Relation Relationship(CustomRole otherRole)
    {
        return otherRole.MyPlayer.PlayerId == Partner ? Relation.SharedWinners : base.Relationship(otherRole);
    }
}