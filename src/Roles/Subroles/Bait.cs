using TOHTOR.Roles.Internals.Attributes;
using UnityEngine;

namespace TOHTOR.Roles.Subroles;

public class Bait: Subrole
{
    [RoleAction(RoleActionType.MyDeath)]
    private void BaitDies(PlayerControl killer) => killer.ReportDeadBody(MyPlayer.Data);

    public override string? Identifier() => "★";

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0f, 0.7f, 0.7f));

}