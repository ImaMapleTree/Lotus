using TOHTOR.Roles.Overrides;
using UnityEngine;

namespace TOHTOR.Roles.Subroles;

public class Watcher: Subrole
{
    public override string Identifier() => "☯";

    protected override RoleModifier Modify(RoleModifier roleModifier) => 
        base.Modify(roleModifier).OptionOverride(Override.AnonymousVoting, false).RoleColor(new Color(0.48f, 0.53f, 0.61f));
}