using System.Collections.Generic;
using Lotus.Utilities;
using VentLib.Options.Game;
using VentLib.Options.Game.Tabs;
using VentLib.Utilities.Attributes;

namespace Lotus.Options;

[LoadStatic]
public class DefaultTabs
{
    public static GameOptionTab GeneralTab = new("General Settings", () => AssetLoader.LoadSprite("Lotus.assets.TabIcons.GeneralIcon.png", 650, true));

    public static GameOptionTab ImpostorsTab = new("Impostor Settings", () => AssetLoader.LoadSprite("Lotus.assets.TabIcons.ImpostorsIcon.png", 650, true));

    public static GameOptionTab CrewmateTab = new("Crewmate Settings", () => AssetLoader.LoadSprite("Lotus.assets.TabIcons.CrewmatesIcon.png", 650, true));

    public static GameOptionTab NeutralTab = new("Neutral Settings", () => AssetLoader.LoadSprite("Lotus.assets.TabIcons.NeutralsIcon.png", 650, true));

    //public static GameOptionTab SubrolesTab = new("Subrole Settings", "Lotus.assets.TabIcon_Addons.png");

    public static GameOptionTab MiscTab = new("Misc Settings", () => AssetLoader.LoadSprite("Lotus.assets.TabIcons.MiscIcon.png", 650, true));

    public static GameOptionTab HiddenTab = new("Hidden", () => AssetLoader.LoadSprite("Lotus.assets.TabIcon_Addons.png"));

    public static List<GameOptionTab> All = new() { GeneralTab, ImpostorsTab, CrewmateTab, NeutralTab, MiscTab };

    static DefaultTabs()
    {
        GameOptionController.AddTab(GeneralTab);
        GameOptionController.AddTab(ImpostorsTab);
        GameOptionController.AddTab(CrewmateTab);
        GameOptionController.AddTab(NeutralTab);
        GameOptionController.AddTab(MiscTab);
    }
}