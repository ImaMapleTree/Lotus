using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.Gamemodes.Colorwars;
using Lotus.Gamemodes.Debug;
using Lotus.Gamemodes.Standard;
using Lotus.API;
using Lotus.Gamemodes.CaptureTheFlag;
using VentLib.Options;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Options.Game.Events;
using VentLib.Options.Game.Tabs;
using VentLib.Utilities.Extensions;

namespace Lotus.Gamemodes;

// As we move to the future we're going to try to use instances for managers rather than making everything static
public class GamemodeManager
{
    private const string GamemodeManagerStartHook = nameof(GamemodeManager);

    public List<IGamemode> Gamemodes = new();

    public IGamemode CurrentGamemode
    {
        get => currentGamemode!;
        set
        {
            currentGamemode?.InternalDeactivate();
            currentGamemode = value;
            currentGamemode?.InternalActivate();
        }
    }

    private IGamemode? currentGamemode;
    private Option gamemodeOption = null!;
    internal readonly List<Type> GamemodeTypes = new() { typeof(StandardGamemode)};

    public GamemodeManager()
    {
        Hooks.GameStateHooks.GameStartHook.Bind(GamemodeManagerStartHook, _ => CurrentGamemode.SetupWinConditions(Game.GetWinDelegate()));
    }


    public void SetGamemode(int id)
    {
        CurrentGamemode = Gamemodes[id];
        VentLogger.High($"Setting Gamemode {CurrentGamemode.GetName()}", "Gamemode");
    }

    public void Setup()
    {
        Gamemodes = GamemodeTypes.Select(g => (IGamemode)g.GetConstructor(Array.Empty<Type>())!.Invoke(null)).ToList();

        GameOptionBuilder builder = new GameOptionBuilder()
            .Name("Gamemode")
            .IsHeader(true)
            .Tab(VanillaMainTab.Instance)
            .BindInt(SetGamemode);

        for (int i = 0; i < Gamemodes.Count; i++)
        {
            IGamemode gamemode = Gamemodes[i];
            var index = i;
            builder.Value(v => v.Text(gamemode.GetName()).Value(index).Build());
        }

        gamemodeOption = builder.BuildAndRegister();
        GameOptionController.RegisterEventHandler(ce =>
        {
            if (ce is not OptionOpenEvent) return;
            GameOptionController.ClearTabs();
            currentGamemode.EnabledTabs().ForEach(GameOptionController.AddTab);
        });
    }
}