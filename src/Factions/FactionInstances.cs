using TOHTOR.Factions.Crew;
using TOHTOR.Factions.Impostors;
using TOHTOR.Factions.Neutrals;
using TOHTOR.Factions.Undead;

namespace TOHTOR.Factions;

public class FactionInstances
{
    public static Crewmates Crewmates { get; } = new();
    public static ImpostorFaction Impostors { get; } = new();
    public static Madmates Madmates { get; } = new();
    public static TheUndead TheUndead { get; } = new TheUndead.Origin();
    public static Solo Solo { get; } = new();
}