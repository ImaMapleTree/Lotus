using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.API;
using Lotus.Extensions;
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

        tasksRemaining = this.player.GetCustomRole() is Crewmate crew
            ? crew.TotalTasks - crew.TasksComplete
            : player.Data.Tasks.ToArray().Count(t => !t.Complete);
    }

    public PlayerControl Player() => player;

    public Optional<CustomRole> RelatedRole() => playerRole;

    public Timestamp Timestamp() => timestamp;

    public bool IsCompletion() => true;

    public string Message() => $"{Game.GetName(player)} completed a task.";

    public int TasksRemaining() => tasksRemaining;
}