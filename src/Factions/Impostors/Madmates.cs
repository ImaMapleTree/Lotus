using TOHTOR.Factions.Interfaces;

namespace TOHTOR.Factions.Impostors;

public class Madmates : ImpostorFaction, ISubFaction<ImpostorFaction>
{
    public Relation MainFactionRelationship() => Relation.SharedWinners;

    public Relation Relationship(ISubFaction<ImpostorFaction> subFaction)
    {
        return subFaction is Madmates ? Relation.SharedWinners : subFaction.Relationship(this);
    }
}