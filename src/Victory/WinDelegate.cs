using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.Victory.Conditions;
using Lotus.Extensions;
using Lotus.Options;
using VentLib.Utilities.Extensions;
using VentLib.Logging;
using VentLib.Utilities.Collections;

namespace Lotus.Victory;

public class WinDelegate
{
    private readonly List<IWinCondition> winConditions = new() { new FallbackCondition() };
    private readonly RemoteList<Action<WinDelegate>> winNotifiers = new();

    private IWinCondition? winCondition;

    private List<PlayerControl> winners = new();
    private WinReason winReason;
    private bool forcedWin;
    private bool forcedCancel;

    public IWinCondition? WinCondition() => winCondition;
    public WinReason GetWinReason() => winReason;
    public void SetWinReason(WinReason reason) => winReason = reason;
    public List<PlayerControl> GetWinners() => winners;
    public void SetWinners(List<PlayerControl> winners) => this.winners = winners;

    public bool IsGameOver()
    {
        if (forcedWin)
        {
            VentLogger.Info($"Triggering Game Win by Force, winners={winners.Where(p => p != null).Select(p => p.name).Join()}, reason={winReason}", "WinCondition");
            winNotifiers.ForEach(notify => notify(this));
            return true;
        }

        IWinCondition? condition = winConditions.FirstOrDefault(con => con.IsConditionMet(out winners));
        if (condition == null) return false;
        if (winners == null!)
        {
            VentLogger.Warn("The list of winners was null. Please do ensure that the winner list is not null if the win condition is actually met.");
            return false;
        }

        winCondition = condition;
        winNotifiers.ForEach(notify => notify(this));

        if (forcedCancel)
        {
            winCondition = null;
            forcedCancel = false;
            return false;
        }

        winReason = condition.GetWinReason();
        VentLogger.Info($"Triggering Win by \"{condition.GetType()}\", winners={winners.Where(p => p != null).Select(p => p.name).StrJoin()}, reason={winReason}", "WinCondition");
        return true;
    }

    /// <summary>
    /// Adds a consumer which gets triggered when the game has detected a possible win. This allows for pre-win interactions
    /// as well as the possibility to cancel a game win via CancelGameWin() or to modify the game winners
    /// </summary>
    /// <param name="consumer"></param>
    public Remote<Action<WinDelegate>> AddSubscriber(Action<WinDelegate> consumer)
    {
        return winNotifiers.Add(consumer);
    }

    public void AddWinCondition(IWinCondition condition)
    {
        winConditions.Add(condition);
        winConditions.Sort();
    }

    public void ForceGameWin(List<PlayerControl> forcedWinners, WinReason reason)
    {
        this.winners = forcedWinners;
        this.winReason = reason;
        forcedWin = true;
    }

    public void CancelGameWin()
    {
        forcedCancel = true;
    }
}