using System.Collections.Generic;
using Lotus.Victory;
using VentLib.Options.Game.Tabs;

namespace Lotus.GameModes.Standard;

public class StandardGameMode: GameMode
{
    public static StandardGameMode Instance;

    public override string Name { get; set; } = "Standard";

    public StandardGameMode()
    {
        Instance = this;
    }

    public override IEnumerable<GameOptionTab> EnabledTabs()
    {
        throw new System.NotImplementedException();
    }

    public override void Setup()
    {
        throw new System.NotImplementedException();
    }

    public override void SetupWinConditions(WinDelegate winDelegate)
    {
        throw new System.NotImplementedException();
    }

    public override void AssignRoles(List<PlayerControl> players)
    {
        throw new System.NotImplementedException();
    }
}