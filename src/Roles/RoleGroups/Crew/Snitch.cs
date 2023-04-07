using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Factions.Impostors;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.NeutralKilling;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

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
            Color color = ArrowIsColored ? p.GetCustomRole().RoleColor : Color.white;
            LiveString liveString = new(() => RoleUtils.CalculateArrow(MyPlayer, p, color));
            var remote = MyPlayer.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(liveString, GameState.Roaming, viewers: MyPlayer));
            indicatorComponents.Add(remote);
            p.NameModel().GetComponentHolder<RoleHolder>().Components().ForEach(rc => rc.AddViewer(MyPlayer));
        });
    }

    private bool IsTrackable(PlayerControl player)
    {
        if (player.GetCustomRole().Faction is ImpostorFaction) return true;
        if (!SnitchCanTrackNk) return false;
        return player.GetCustomRole() is NeutralKillingBase;
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) => roleModifier.RoleColor(new Color(0.72f, 0.98f, 0.31f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddTaskOverrideOptions(base.RegisterOptions(optionStream)
            .SubOption(s => s.Name("Remaining Task Warning")
                .AddIntRange(0, 10, 1, 2)
                .BindInt(i => SnitchWarningTasks = i)
                .Build())
            .SubOption(s => s.Name("Evil Have Arrow to Snitch")
                .AddOnOffValues()
                .BindBool(b => EvilHaveArrow = b)
                .Build())
            .SubOption(s => s.Name("Enable Arrow for Snitch")
                .BindBool(v => SnitchHasArrow = v)
                .AddOnOffValues()
                .ShowSubOptionPredicate(o => (bool)o)
                .SubOption(arrow => arrow.Name("Colored Arrow")
                    .BindBool(v => ArrowIsColored = v)
                    .AddOnOffValues()
                    .Build())
                .Build())
            .SubOption(s => s.Name("Snitch Can Track Any Killing")
                .BindBool(v => SnitchCanTrackNk = v)
                .AddOnOffValues()
                .Build()));
}