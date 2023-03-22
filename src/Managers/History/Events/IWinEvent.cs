using System.Collections.Generic;
using TOHTOR.Victory.Conditions;

namespace TOHTOR.Managers.History.Events;

public interface IWinEvent : IHistoryEvent
{
    public WinReason WinReason();

    public List<PlayerControl> Winners();
}