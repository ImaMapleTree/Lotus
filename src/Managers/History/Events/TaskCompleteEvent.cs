using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.Extensions;
using Lotus.Roles.Interfaces;
using VentLib.Utilities.Optionals;

namespace Lotus.Managers.History.Events;

public class TaskCompleteEvent : IHistoryEvent
{
    private PlayerControl player;
    private Optional<CustomRole> playerRole;

    private int tasksRemaining;
    private Timestamp timestamp = new();

    public TaskCompleteEvent(PlayerControl player)
    {
        this.player = player;
        playerRole = Optional<CustomRole>.Of(player.GetCustomRole());

        tasksRemaining = this.player.GetCustomRole() is ITaskHolderRole taskHolderRole
            ? taskHolderRole.TotalTasks - taskHolderRole.CompleteTasks
            : player.Data.Tasks.ToArray().Count(t => !t.Complete);
    }

    public PlayerControl Player() => player;

    public Optional<CustomRole> RelatedRole() => playerRole;

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => true;

    public string Message() => $"{Game.GetName(player)} completed a task.";

    public int TasksRemaining() => tasksRemaining;
}