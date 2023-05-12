using AmongUs.GameOptions;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.Options;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.Overrides;
using TOHTOR.Victory.Conditions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.Neutral;

[Localized($"Roles.{nameof(Vulture)}")]
public class Vulture : CustomRole
{
    private static Color _modeColor = new(0.73f, 0.18f, 0.02f);
    
    private int bodyCount;
    private int bodyAmount;
    private bool canUseVents;
    private bool impostorVision;
    private bool canSwitchMode;
    private bool isEatMode = true;
    
    [Localized("EatingModeText")]
    private static string _eatingModeText = "Feasting";

    [Localized("ReportingModeText")]
    private static string _reportingModeText = "Reporting";

    [UIComponent(UI.Counter)]
    private string BodyCounter() => RoleUtils.Counter(bodyCount, bodyAmount, RoleColor);
    
    [UIComponent(UI.Text)]
    private string DisplayModeText() => canSwitchMode ? _modeColor.Colorize(isEatMode ? _eatingModeText : _reportingModeText) : "";
    
    [RoleAction(RoleActionType.SelfReportBody)]
    private void EatBody(GameData.PlayerInfo body, ActionHandle handle)
    {
        Game.MatchData.UnreportableBodies.Add(body.PlayerId);

        if (++bodyCount >= bodyAmount) ManualWin.Activate(MyPlayer, WinReason.RoleSpecificWin, 100);

        handle.Cancel();
    }

    [RoleAction(RoleActionType.OnPet)]
    public void Switch()
    {
        if (!canSwitchMode) return;
        isEatMode = !isEatMode;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
        .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => sub
                .Name("Required Bodies")
                .Bind(v => bodyAmount = (int)v)
                .AddIntRange(1, 10, 1, 2)
                .Build())
            .SubOption(opt =>
                opt.Name("Has Impostor Vision")
                .Bind(v => impostorVision = (bool)v)
                .AddOnOffValues()
                .Build())
            .SubOption(opt =>
                opt.Name("Can Switch between Eat and Report")
                .Bind(v => canSwitchMode = (bool)v)
                .AddOnOffValues()
                .Build())
            .SubOption(opt => opt.Name("Can Use Vents")
                .Bind(v => canUseVents = (bool)v)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(0.64f, 0.46f, 0.13f))
            .Faction(FactionInstances.Solo)
            .VanillaRole(canUseVents ? RoleTypes.Engineer : RoleTypes.Crewmate)
            .CanVent(canUseVents)
            .SpecialType(SpecialType.Neutral)
            .OptionOverride(Override.CrewLightMod, () => AUSettings.ImpostorLightMod(), () => impostorVision);
}