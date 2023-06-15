namespace Lotus.Roles.Subroles.Romantics;

public class Partner: Subrole
{
    public override string Identifier() => null!;

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(Romantic.RomanticColor)
            .RoleFlags(RoleFlag.Hidden | RoleFlag.Unassignable | RoleFlag.DoNotTranslate | RoleFlag.DontRegisterOptions);
}