using TOHTOR.Factions.Interfaces;

namespace TOHTOR.Factions.Undead;

public partial class TheUndead
{
    public class Origin : TheUndead, ISubFaction<TheUndead>
    {
        public override bool AlliesSeeRole() => true;

        public Relation MainFactionRelationship() => Relation.FullAllies;

        public Relation Relationship(ISubFaction<TheUndead> subFaction)
        {
            return subFaction switch
            {
                Converted => Relation.FullAllies,
                Unconverted => Relation.SharedWinners,
                _ => subFaction.Relationship(this)
            };
        }
    }
}