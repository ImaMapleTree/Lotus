using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.Managers.History.Events;
using Lotus.Player;
using Lotus.API.Odyssey;
using Lotus.Roles;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History;

public class GameHistory
{
    public List<FrozenPlayer> LastWinners { get; internal set; } = new();
    public List<PlayerHistory>? PlayerHistory;
    public readonly List<IHistoryEvent> Events = new();

    private readonly Dictionary<byte, IDeathEvent> causeOfDeath = new();
    private static readonly List<Action<IHistoryEvent>> EventSubscribers = new();

    public static void AddEventSubscriber(Action<IHistoryEvent> eventSubscriber) => EventSubscribers.Add(eventSubscriber);

    public void AddEvent(IHistoryEvent historyEvent)
    {
        Events.Add(historyEvent);
        EventSubscribers.ForEach(subscriber => subscriber(historyEvent));
    }

    public void SetCauseOfDeath(byte playerId, IDeathEvent deathEvent) => causeOfDeath[playerId] = deathEvent;

    public Optional<IDeathEvent> GetCauseOfDeath(byte playerId) => causeOfDeath.GetOptional(playerId);

    public List<T> GetEvents<T>() where T : IHistoryEvent => Events.Where(item => item.GetType() == typeof(T)).Cast<T>().ToList();
}