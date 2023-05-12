using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles.Interfaces;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.Overrides;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.Impostors;

public partial class SerialKiller : Impostor, IModdable
{
    private bool paused = true;
    public Cooldown DeathTimer = null!;
    private float killCooldown;
    private bool beginsAfterFirstKill;

    private bool hasKilled;

    [UIComponent(UI.Counter)]
    private string CustomCooldown() => DeathTimer.IsReady() ? "" : Color.white.Colorize(DeathTimer + "s");

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        bool success = base.TryKill(target);
        if (!success) return false;

        hasKilled = true;
        paused = false;
        DeathTimer.Start();
        return success;
    }

    [RoleAction(RoleActionType.FixedUpdate)]
    private void CheckForSuicide()
    {
        if (MyPlayer == null) return;
        if (paused || DeathTimer.NotReady() || !MyPlayer.IsAlive()) return;

        if (Game.State is GameState.InMeeting)
        {
            paused = true;
            return;
        }
        
        VentLogger.Trace($"Serial Killer ({MyPlayer.name}) Commiting Suicide", "SerialKiller::CheckForSuicide");
        
        MyPlayer.RpcMurderPlayer(MyPlayer);
        Game.MatchData.GameHistory.AddEvent(new SuicideEvent(MyPlayer));
    }

    [RoleAction(RoleActionType.RoundStart)]
    private void SetupSuicideTimer()
    {
        paused = beginsAfterFirstKill && !hasKilled;
        if (!paused) DeathTimer.Start();
    }

    [RoleAction(RoleActionType.RoundEnd)]
    private void StopDeathTimer() => paused = true;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream), defaultIndex: 4)
            .SubOption(sub => sub
                .KeyName("Time Until Suicide", SerialKillerTranslations.SerialKillerOptionTranslations.TimeUntilSuicide)
                .Bind(v => DeathTimer.Duration = (float)v)
                .AddFloatRange(5, 120, 2.5f, 30, "s")
                .Build())
            .SubOption(sub => sub
                .KeyName("Timer Begins After First Kill", SerialKillerTranslations.SerialKillerOptionTranslations.TimerAfterFirstKill)
                .BindBool(b => beginsAfterFirstKill = b)
                .AddOnOffValues(false)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).OptionOverride(Override.KillCooldown, () => killCooldown);


    [Localized(nameof(SerialKiller))]
    private static class SerialKillerTranslations
    {
        [Localized("Options")]
        public static class SerialKillerOptionTranslations
        {
            [Localized(nameof(TimeUntilSuicide))]
            public static string TimeUntilSuicide = "Time Until Suicide";
            
            [Localized(nameof(TimerAfterFirstKill))]
            public static string TimerAfterFirstKill = "Timer Begins After First Kill";
        }
    }
}