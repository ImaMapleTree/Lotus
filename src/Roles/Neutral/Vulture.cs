using System.Collections.Generic;
using TownOfHost.Extensions;
using TownOfHost.Options;
using AmongUs.GameOptions;
using TownOfHost.Managers;
using TownOfHost.Victory.Conditions;

namespace TownOfHost.Roles;

public class Vulture : CustomRole
{
    private int bodyCount = 0;
    // option
    private int bodyAmount;
    private bool canUseVents;
    private bool impostorVision;
    private bool canSwitchMode;
    private bool isEatMode = true;

    [RoleAction(RoleActionType.SelfReportBody)]
    private void EatBody(GameData.PlayerInfo body, ActionHandle handle)
    {
        List<byte> blockedBodies = Game.GameStates.UnreportableBodies;
        if (!isEatMode || blockedBodies.Contains(body.PlayerId)) return;
        blockedBodies.Add(body.PlayerId);

        if (++bodyCount >= bodyAmount)
           new ManualWin(MyPlayer, WinReason.RoleSpecificWin).Activate();

        handle.Cancel();
    }

    [RoleAction(RoleActionType.OnPet)]
    public void Switch()
    {
        if (!canSwitchMode) return;
        isEatMode = !isEatMode;
    }

    protected override SmartOptionBuilder RegisterOptions(SmartOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
        .Tab(DefaultTabs.NeutralTab)
            .AddSubOption(sub => sub
                .Name("Required Bodies")
                .Bind(v => bodyAmount = (int)v)
                .AddIntRangeValues(1, 10, 1, 2)
                .Build())
            .AddSubOption(opt =>
                opt.Name("Has Impostor Vision")
                .Bind(v => impostorVision = (bool)v)
                .AddOnOffValues()
                .Build())
            .AddSubOption(opt =>
                opt.Name("Can Switch between Eat and Report")
                .Bind(v => canSwitchMode = (bool)v)
                .AddOnOffValues()
                .Build())
            .AddSubOption(opt => opt.Name("Can Use Vents")
                .Bind(v => canUseVents = (bool)v)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor("#a36727")
        .VanillaRole(canUseVents ? RoleTypes.Engineer : RoleTypes.Crewmate)
        .SpecialType(SpecialType.Neutral)
        .OptionOverride(Override.CrewLightMod,
            () => GameOptionsManager.Instance.CurrentGameOptions.AsNormalOptions()!.ImpostorLightMod, () => impostorVision);
}