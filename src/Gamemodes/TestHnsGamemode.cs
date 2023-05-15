using System.Collections.Generic;
using Lotus.Options;
using Lotus.Utilities;
using Lotus.Victory;
using VentLib.Options.Game.Tabs;

namespace Lotus.Gamemodes;

public class TestHnsGamemode: Gamemode
{
    public override string GetName() => "Hide and Seek";

    public static GameOptionTab HnsTab = new("Hide & Seek Options", () => Utils.LoadSprite("Lotus.assets.TabIcons.HideAndSeekIcon.png", 675));

    public override void Setup()
    {
    }

    public override void AssignRoles(List<PlayerControl> players)
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerable<GameOptionTab> EnabledTabs() => new[] { DefaultTabs.GeneralTab, HnsTab };

    public override void SetupWinConditions(WinDelegate winDelegate)
    {
        throw new System.NotImplementedException();
    }
}