using TOHTOR.Factions.Interfaces;
using TOHTOR.GUI.Name.Components;

namespace TOHTOR.Factions.Undead;

public partial class TheUndead
{
    public class Unconverted : TheUndead, ISubFaction<TheUndead>
    {
        public IFaction PreviousFaction { get; }
        public NameComponent UnconvertedName { get; }

        public Unconverted(IFaction previousFaction, NameComponent unconvertedName)
        {
            this.PreviousFaction = previousFaction;
            this.UnconvertedName = unconvertedName;
        }

        public Relation MainFactionRelationship() => Relation.SharedWinners;

        public Relation Relationship(ISubFaction<TheUndead> subFaction)
        {
            return subFaction switch
            {
                Origin => Relation.SharedWinners,
                Converted => Relation.SharedWinners,
                Unconverted => Relation.SharedWinners,
                _ => subFaction.Relationship(this)
            };
        }

        public override bool AlliesSeeRole() => false;
    }
}