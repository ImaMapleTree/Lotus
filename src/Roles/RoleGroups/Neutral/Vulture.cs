using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Victory.Conditions;
using Lotus.Extensions;
using Lotus.Roles.Subroles;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using static Lotus.Roles.RoleGroups.Neutral.Vulture.Translations.Options;
using Object = UnityEngine.Object;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Vulture : CustomRole
{
    public static HashSet<Type> VultureBannedModifiers = new() { typeof(Oblivious), typeof(Sleuth) };
    public override HashSet<Type> BannedModifiers() => canSwitchMode ? new HashSet<Type>() : VultureBannedModifiers;
    private static Color _modeColor = new(0.73f, 0.18f, 0.02f);

    private int bodyCount;
    private int bodyAmount;
    private bool canUseVents;
    private bool impostorVision;
    private bool canSwitchMode;
    private bool hasArrowsToBodies;
    private bool isEatMode = true;



    [UIComponent(UI.Counter)]
    private string BodyCounter() => RoleUtils.Counter(bodyCount, bodyAmount, RoleColor);

    [UIComponent(UI.Text)]
    private string DisplayModeText() => canSwitchMode ? _modeColor.Colorize(isEatMode ? Translations.EatingModeText : Translations.ReportingModeText) : "";

    [UIComponent(UI.Indicator)]
    private string Arrows() => hasArrowsToBodies ? Object.FindObjectsOfType<DeadBody>()
        .Where(b => !Game.MatchData.UnreportableBodies.Contains(b.ParentId))
        .Select(b => RoleUtils.CalculateArrow(MyPlayer, b.TruePosition, RoleColor)).Fuse("") : "";

    [RoleAction(RoleActionType.SelfReportBody)]
    private void EatBody(GameData.PlayerInfo body, ActionHandle handle)
    {
        Game.MatchData.UnreportableBodies.Add(body.PlayerId);

        if (++bodyCount >= bodyAmount) ManualWin.Activate(MyPlayer, ReasonType.RoleSpecificWin, 100);

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
                .KeyName("Required Bodies", RequiredBodies)
                .BindInt(v => bodyAmount = v)
                .AddIntRange(1, 10, 1, 2)
                .Build())
            .SubOption(opt =>
                opt.KeyName("Has Impostor Vision", HasImpostorVision)
                .BindBool(v => impostorVision = v)
                .AddOnOffValues()
                .Build())
            .SubOption(opt =>
                opt.KeyName("Can Switch between Eat and Report", SwitchModes)
                .BindBool(v => canSwitchMode = v)
                .AddOnOffValues()
                .Build())
            .SubOption(opt => opt.KeyName("Can Use Vents", CanUseVent)
                .BindBool(v => canUseVents = v)
                .AddOnOffValues()
                .Build())
            .SubOption(sub => sub.KeyName("Has Arrow To Bodies", HasArrowsToBody)
                .BindBool(b => hasArrowsToBodies = b)
                .AddOnOffValues()
                .Build());


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(0.64f, 0.46f, 0.13f))
            .Faction(FactionInstances.Neutral)
            .VanillaRole(canUseVents ? RoleTypes.Engineer : RoleTypes.Crewmate)
            .CanVent(canUseVents)
            .SpecialType(SpecialType.Neutral)
            .OptionOverride(Override.CrewLightMod, () => AUSettings.ImpostorLightMod(), () => impostorVision);

    [Localized(nameof(Vulture))]
    internal static class Translations
    {
        [Localized(nameof(EatingModeText))]
        public static string EatingModeText = "Feasting";

        [Localized(nameof(ReportingModeText))]
        public static string ReportingModeText = "Reporting";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(RequiredBodies))]
            public static string RequiredBodies = "Required Bodies";

            [Localized(nameof(HasImpostorVision))]
            public static string HasImpostorVision = "Has Impostor Vision";

            [Localized(nameof(SwitchModes))]
            public static string SwitchModes = "Can Switch between Eat and Report";

            [Localized(nameof(CanUseVent))]
            public static string CanUseVent = "Can Use Vents";

            [Localized(nameof(HasArrowsToBody))]
            public static string HasArrowsToBody = "Has Arrows to Bodies";
        }
    }
}