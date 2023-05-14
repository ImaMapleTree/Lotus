using System;
using System.Collections.Generic;

namespace Lotus.Victory.Conditions;

public interface IWinCondition: IComparable<IWinCondition>
{
    /// <summary>
    /// Called by game system, returns true if win condition criteria has been met, otherwise false.
    /// <br/>
    /// <b>IMPORTANT!</b> This function does NOT run on the main thread! Do not use Unity functions unless you know what you're doing. You have been warned!
    /// </summary>
    /// <param name="winners">the list of winners that met the win criteria</param>
    /// <returns>true if the win condition has been met, otherwise false</returns>
    bool IsConditionMet(out List<PlayerControl> winners);

    /// <summary>
    /// Gets the priority of this win condition, win conditions get checked in order of priority.
    /// So in the case of two win conditions being true, the one with the higher priority will be the only condition ran
    /// </summary>
    /// <returns></returns>
    int Priority() => 0;

    WinReason GetWinReason();

    int IComparable<IWinCondition>.CompareTo(IWinCondition? other) => other.Priority().CompareTo(Priority());
}