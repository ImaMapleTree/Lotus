using Lotus.Factions.Interfaces;
using Lotus.GUI.Name.Components;

namespace Lotus.Factions.Undead;

public partial class TheUndead
{
    public class Converted : TheUndead, ISubFaction<TheUndead>
    {
        public NameComponent NameComponent { get; }

        public Converted(NameComponent component)
        {
            NameComponent = component;
        }

        public Relation MainFactionRelationship() => Relation.FullAllies;

        public Relation Relationship(ISubFaction<TheUndead> subFaction)
        {
            return subFaction switch
            {
                Origin => Relation.FullAllies,
                Converted => Relation.FullAllies,
                Unconverted => Relation.SharedWinners,
                _ => subFaction.Relationship(this)
            };
        }

        public override bool AlliesSeeRole() => true;
    }
}