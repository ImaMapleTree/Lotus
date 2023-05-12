using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Factions.Crew;
using TOHTOR.Factions.Impostors;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.NeutralKilling;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using static TOHTOR.Roles.RoleGroups.Crew.Snitch.SnitchTranslations.SnitchOptionTranslations;
using static TOHTOR.Utilities.TranslationUtil;

namespace TOHTOR.Roles.RoleGroups.Crew;

public class Snitch : Crewmate
{
    public bool SnitchCanTrackNk;

    public bool EvilHaveArrow;
    public bool SnitchHasArrow;
    public bool ArrowIsColored;

    public int SnitchWarningTasks = 2;

    [NewOnSetup] private List<Remote<IndicatorComponent>> indicatorComponents = null!;

    [RoleAction(RoleActionType.MyDeath)]
    private void ClearComponents() => indicatorComponents.ForEach(c => c.Delete());

    protected override void OnTaskComplete()
    {
        int remainingTasks = TotalTasks - TasksComplete;
        if (remainingTasks == SnitchWarningTasks)
        {
            MyPlayer.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(new LiveString("★", RoleColor), GameStates.IgnStates));
            if (EvilHaveArrow)
                Game.GetAlivePlayers().Where(IsTrackable).ForEach(p =>
                {
                    LiveString liveString = new(() => RoleUtils.CalculateArrow(p, MyPlayer, RoleColor));
                    var remote = p.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(liveString, GameState.Roaming, viewers: p));
                    indicatorComponents.Add(remote);
                });
        }

        if (remainingTasks != 0) return;
        Game.GetAlivePlayers().Where(IsTrackable).ForEach(p =>
        {
            p.NameModel().GetComponentHolder<RoleHolder>().Components().ForEach(rc => rc.AddViewer(MyPlayer));

            if (!SnitchHasArrow) return;
            
            Color color = ArrowIsColored ? p.GetCustomRole().RoleColor : Color.white;
            LiveString liveString = new(() => RoleUtils.CalculateArrow(MyPlayer, p, color));
            var remote = MyPlayer.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(liveString, GameState.Roaming, viewers: MyPlayer));
            indicatorComponents.Add(remote);
        });
    }

    private bool IsTrackable(PlayerControl player)
    {
        CustomRole role = player.GetCustomRole();
        if (role.Faction is ImpostorFaction) return true;
        if (!SnitchCanTrackNk) return false;
        return role.Faction is not Crewmates && role.RealRole.IsImpostor();
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.72f, 0.98f, 0.31f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddTaskOverrideOptions(base.RegisterOptions(optionStream)
            .SubOption(s => s.KeyName("Remaining Task Warning", RemainingTaskWarning)
                .AddIntRange(0, 10, 1, 2)
                .BindInt(i => SnitchWarningTasks = i)
                .Build())
            .SubOption(s => s.KeyName("Evil Have Arrow to Snitch", Colorize(EvilArrowToSnitch, RoleColor))
                .AddOnOffValues()
                .BindBool(b => EvilHaveArrow = b)
                .Build())
            .SubOption(s => s.KeyName("Enable Arrow for Snitch", Colorize(SnitchArrow, RoleColor))
                .BindBool(v => SnitchHasArrow = v)
                .AddOnOffValues()
                .ShowSubOptionPredicate(o => (bool)o)
                .SubOption(arrow => arrow.KeyName("Colored Arrow", ColoredArrow)
                    .BindBool(v => ArrowIsColored = v)
                    .AddOnOffValues()
                    .Build())
                .Build())
            .SubOption(s => s.KeyName("Snitch Can Track Any Killing", Colorize(SnitchTracksAllKillers, RoleColor))
                .BindBool(v => SnitchCanTrackNk = v)
                .AddOnOffValues()
                .Build()));


    [Localized(nameof(Snitch))]
    internal static class SnitchTranslations
    {
        [Localized("Options")]
        internal static class SnitchOptionTranslations
        {
            [Localized(nameof(RemainingTaskWarning))]
            public static string RemainingTaskWarning = "Remaining Task Warning";

            [Localized(nameof(EvilArrowToSnitch))]
            public static string EvilArrowToSnitch = "Evil Have Arrow to Snitch::0";

            [Localized(nameof(SnitchArrow))]
            public static string SnitchArrow = "Enable Arrow for Snitch::0";

            [Localized(nameof(ColoredArrow))]
            public static string ColoredArrow = "Colored Arrow";

            [Localized(nameof(SnitchTracksAllKillers))]
            public static string SnitchTracksAllKillers = "Snitch::0 Can Track Any Killing";
        }
    }
}