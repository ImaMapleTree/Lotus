using System.Collections.Generic;

namespace TOHTOR.FactionsOLD;

// Used to validate that all factions have unique long IDs
public class FactionConstraintValidator
{
    public static Dictionary<ulong, string> uniqueFactionMap = new();

    static FactionConstraintValidator()
    {
        /*Enum.GetValues<IFaction>().Do(f => uniqueFactionMap.Add((ulong)f, "BaseMod"));*/
    }

    /*public static void ValidateAndAdd(IFaction factionOld, string addonName)
    {
        if (uniqueFactionMap.TryGetValue((ulong)factionOld, out string ownerName))
            throw new ConstraintException($"Faction ID: {(ulong)factionOld} has already been registered by \"{ownerName}\". All factions must have unique IDs. Please choose a random number between 0 - 4,294,967,295 for your faction. If you are not the developer of this Addon. Please contact the developer about this issue.");

        uniqueFactionMap.Add((ulong) factionOld, addonName);
    }*/

}