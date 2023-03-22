namespace TOHTOR.Roles.Subroles;

public abstract class Subrole: CustomRole
{
    public abstract string? Identifier();

    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier.Subrole(true);
}
