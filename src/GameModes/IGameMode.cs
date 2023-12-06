using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.Managers;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using Lotus.Roles2.Manager;
using Lotus.Victory;
using VentLib.Options.Game;
using VentLib.Options.Game.Tabs;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.GameModes;

public interface IGameMode
{
    public string Name { get; set; }
    public CoroutineManager CoroutineManager { get; }
    public IRoleManager RoleManager { get; }
    public MatchData MatchData { get; protected internal set; }

    IEnumerable<GameOptionTab> EnabledTabs();

    void AssignRoles(List<PlayerControl> players);

    protected internal void Activate();

    protected internal void Deactivate();

    protected internal void FixedUpdate();

    protected internal void Setup();

    protected internal void SetupWinConditions(WinDelegate winDelegate);

    Remote<GameOptionOverride> AddOverride(byte playerId, GameOptionOverride optionOverride) => MatchData.Roles.AddOverride(playerId, optionOverride);

    internal void InternalActivate()
    {
        Activate();
        EnabledTabs().ForEach(GameOptionController.AddTab);
    }

    internal void InternalDeactivate()
    {
        Deactivate();
        EnabledTabs().ForEach(GameOptionController.RemoveTab);
    }

    void Trigger(LotusActionType action, ActionHandle handle, params object[] arguments);
}