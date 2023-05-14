using Lotus.Factions;
using Lotus.Options;
using Lotus.Roles.Internals.Attributes;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Options.Game;

namespace Lotus.Roles.Extra;

public class GM : CustomRole
{
    public static Color GMColor = new(1f, 0.4f, 0.4f);

    [RoleAction(RoleActionType.RoundStart)]
    public void ExileGM(bool isGameStart)
    {
        if (isGameStart) MyPlayer.RpcExileV2();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream).Tab(DefaultTabs.HiddenTab);

    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier.RoleColor(GMColor).Faction(FactionInstances.Solo);
}