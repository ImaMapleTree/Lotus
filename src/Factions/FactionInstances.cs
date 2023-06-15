using Lotus.Factions.Crew;
using Lotus.Factions.Impostors;
using Lotus.Factions.Neutrals;
using Lotus.Factions.Undead;

namespace Lotus.Factions;

public class FactionInstances
{
    public static Crewmates Crewmates { get; } = new();
    public static ImpostorFaction Impostors { get; } = new();
    public static Madmates Madmates { get; } = new();
    public static TheUndead TheUndead { get; } = new TheUndead.Origin();
    public static Neutral Neutral { get; } = new();
    public static Modifiers Modifiers { get; } = new();
}