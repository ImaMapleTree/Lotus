using Lotus.Factions.Interfaces;
using Lotus.Roles;
using UnityEngine;
using VentLib.Localization.Attributes;

namespace Lotus.Factions;

[Localized("Factions.Modifiers")]
public class Modifiers: IFaction<Modifiers>
{
    [Localized(nameof(Name))]
    private static string name = "Modifiers";

    public string Name() => name;

    public Relation Relationship(Modifiers sameFaction) => Relation.None;

    public Relation Relationship(IFaction other) => Relation.None;

    public Relation Relationship(CustomRole otherRole) => Relation.None;

    public bool CanSeeRole(PlayerControl player) => false;

    public Color Color => new(0.45f, 1f, 0.7f);
}