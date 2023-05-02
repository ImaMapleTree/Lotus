using TOHTOR.Options;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Options.Game;

namespace TOHTOR.Roles.Extra;

public class GM : Crewmate
{
    public static Color GMColor = new Color(1f, 0.4f, 0.4f);


    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream).Tab(DefaultTabs.HiddenTab);

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(GMColor);
}