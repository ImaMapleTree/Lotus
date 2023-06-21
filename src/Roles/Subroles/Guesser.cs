using Lotus.Roles.Interfaces;
using Lotus.Roles.RoleGroups.Stock;
using UnityEngine;

namespace Lotus.Roles.Subroles;

public class Guesser: GuesserRoleBase, ISubrole
{
    public static Color Color = new(0.83f, 1f, 0.42f);
    public string Identifier() => "Ω";

    public virtual bool IsAssignableTo(PlayerControl player) => true;

    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier.RoleColor(Color).RoleName("Guesser").RoleFlags(RoleFlag.IsSubrole | RoleFlag.DontRegisterOptions);
}