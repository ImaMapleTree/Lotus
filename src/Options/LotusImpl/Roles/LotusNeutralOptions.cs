namespace Lotus.Options.LotusImpl.Roles;

public class LotusNeutralOptions: LotusOptionModel
{
    public bool NeutralsKnowAlliedRoles;

    public NeutralTeaming NeutralTeamingMode;
    public bool KnowAlliedRoles => NeutralTeamingMode is not NeutralTeaming.Disabled && KnowAlliedRolesProtected;

    protected bool KnowAlliedRolesProtected;
}

public enum NeutralTeaming
{
    Disabled,
    SameRole,
    KillersNeutrals,
    All
}