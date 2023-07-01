namespace Lotus.Roles.Internals.Enums;

/// <summary>
/// A structure representing a "type" of common roles.
/// <list type="bullet">
/// <item>Impostors</item>
/// <item>Crewmates</item>
/// <item>NeutralKillers</item>
/// <item>NeutralPassives</item>
/// <item>Modifiers</item>
/// </list>
/// </summary>
public struct LotusRoleType
{
    public const ulong ImpostorsTypeID = 0;
    public const ulong CrewmatesTypeID = 1;
    public const ulong NeutralKillersTypeID = 2;
    public const ulong NeutralPassivesTypeID = 3;
    public const ulong ModifiersTypeID = 4;
    internal const ulong InternalTypeID = 100;
    public const ulong UnknownTypeID = ulong.MaxValue;

    public static LotusRoleType Impostors = new("Impostors", ImpostorsTypeID);
    public static LotusRoleType Crewmates = new("Crewmates", CrewmatesTypeID);
    public static LotusRoleType NeutralKillers = new("NeutralKillers", NeutralKillersTypeID);
    public static LotusRoleType NeutralPassives = new("NeutralPassives", NeutralPassivesTypeID);
    public static LotusRoleType Modifiers = new("Modifiers", ModifiersTypeID);
    internal static LotusRoleType Internals = new("Internals", InternalTypeID);
    internal static LotusRoleType Unknown = new("Unknown", UnknownTypeID);

    /// <summary>
    /// Name of the RoleType, this is essentially optional as it's not used anywhere significant
    /// </summary>
    public string Name;

    /// <summary>
    /// A <b>unique</b> type ID. This ID should be complex and hard to replicate. IDs 0 - 100 should NEVER be used.
    /// </summary>
    public ulong TypeID;

    public LotusRoleType(string name, ulong typeID)
    {
        Name = name;
        TypeID = typeID;
    }

    public override bool Equals(object? obj)
    {
        return obj is LotusRoleType lrt && lrt.TypeID == TypeID;
    }

    public override int GetHashCode()
    {
        return TypeID.GetHashCode();
    }
}