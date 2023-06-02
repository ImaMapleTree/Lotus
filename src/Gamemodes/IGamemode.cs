using System.Collections.Generic;
using Lotus.Victory;
using VentLib.Options.Game;
using VentLib.Options.Game.Tabs;
using VentLib.Utilities.Extensions;

namespace Lotus.Gamemodes;

public interface IGamemode
{
    string GetName();

    GameAction IgnoredActions();

    IEnumerable<GameOptionTab> EnabledTabs();

    void Activate();

    void Deactivate();

    void FixedUpdate();

    void AssignRoles(List<PlayerControl> players);

    void Setup();

    void SetupWinConditions(WinDelegate winDelegate);

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

    public void Trigger(GameAction action, params object[] args);
}