using System.Collections.Generic;

namespace Lotus.Managers.History.Events;

public interface IMultiTargetEvent : IHistoryEvent
{
    public List<PlayerControl> Targets();
}