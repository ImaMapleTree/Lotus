using Lotus.Factions.Interfaces;
using VentLib.Localization.Attributes;

namespace Lotus.Factions.Impostors;

[Localized("Factions.Madmates")]
public class Madmates : ImpostorFaction, ISubFaction<ImpostorFaction>
{
    [Localized(nameof(Name))]
    private static string name = "Madmates";

    public override string Name() => name;

    public Relation MainFactionRelationship() => Relation.SharedWinners;

    public Relation Relationship(ISubFaction<ImpostorFaction> subFaction)
    {
        return subFaction is Madmates ? Relation.SharedWinners : subFaction.Relationship(this);
    }
}