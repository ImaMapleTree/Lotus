using System.Collections.Generic;
using Lotus.Victory.Conditions;

namespace Lotus.Managers.History.Events;

public interface IWinEvent : IHistoryEvent
{
    public ReasonType WinReason();

    public List<PlayerControl> Winners();
}