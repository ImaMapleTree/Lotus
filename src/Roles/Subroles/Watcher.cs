using Lotus.Roles.Overrides;
using UnityEngine;

namespace Lotus.Roles.Subroles;

public class Watcher: Subrole
{
    public override string Identifier() => "▲";

    protected override RoleModifier Modify(RoleModifier roleModifier) => 
        base.Modify(roleModifier)
            .OptionOverride(Override.AnonymousVoting, false)
            .RoleColor(new Color(0.38f, 0.51f, 0.61f));
}