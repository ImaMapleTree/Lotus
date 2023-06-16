using Lotus.Factions.Interfaces;
using Lotus.Roles;
using UnityEngine;
using VentLib.Localization.Attributes;

namespace Lotus.Factions;

[Localized("Factions")]
public abstract class Faction<T> : IFaction<T> where T: IFaction<T>
{
    public virtual string Name() => GetType().Name;

    public abstract Relation Relationship(T sameFaction);

    public Relation Relationship(IFaction other)
    {
        if (other is not T self)
        {
            //VentLogger.Info($"Other is not Self ({other} | {typeof(T)}");
            return RelationshipOther(other);
        }

        //VentLogger.Info($"Other is self : {self}");

        if (other is not ISubFaction<T> subFaction2)
        {
            //VentLogger.Info($"Other is not subfaction {other}");
            if (this is ISubFaction<T> subFaction3) return subFaction3.MainFactionRelationship();
            return Relationship(self);
        }

        //VentLogger.Info($"Other is subfaction: {subFaction2}");
        if (this is ISubFaction<T> subFaction)
        {
            //VentLogger.Info($"This is sub faction: {subFaction}");
            return subFaction.Relationship(subFaction2);
        }

        //VentLogger.Info($"this is not subfaction: {this}");
        return subFaction2.MainFactionRelationship();
    }

    public virtual Relation Relationship(CustomRole otherRole) => Relationship(otherRole.Faction);

    public abstract bool CanSeeRole(PlayerControl player);

    public abstract Color Color { get; }

    public abstract Relation RelationshipOther(IFaction other);

    public override string ToString() => Name();
}