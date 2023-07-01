using System.Collections.Generic;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Victory;
using VentLib.Options.Game;
using VentLib.Options.Game.Tabs;
using VentLib.Utilities.Extensions;

namespace Lotus.Gamemodes;

public interface IGamemode
{
    public string Name { get; set; }

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

    void Trigger(LotusActionType action, ref ActionHandle handle, params object[] arguments);
}