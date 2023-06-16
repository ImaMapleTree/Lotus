using Lotus.Factions.Interfaces;
using Lotus.Options;
using UnityEngine;
using VentLib.Localization.Attributes;

namespace Lotus.Factions.Neutrals;

[Localized($"Factions.{nameof(Neutral)}")]
public class Neutral : Faction<Neutral>
{
    [Localized(nameof(Name))]
    private static string _name = "Neutrals";

    private readonly string factionName;

    public Neutral(string? factionName = null)
    {
        this.factionName = factionName ?? _name;
    }

    public override string Name() => this.factionName;

    public override Relation Relationship(Neutral sameFaction) => Relation.None;

    public override bool CanSeeRole(PlayerControl player) => RoleOptions.NeutralOptions.KnowAlliedRoles;

    public override Color Color => Color.gray;

    public override Relation RelationshipOther(IFaction other) => Relation.None;
}