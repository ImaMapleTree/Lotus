using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.Managers.History.Events;
using Lotus.API;
using VentLib.Logging;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.GUI.Menus;

public class HistoryMenuIntermediate
{
    private const string HistoryMenuStartKey = nameof(HistoryMenuIntermediate);

    public static CustomOptional<HistoryMenu> HistoryMenu = CustomOptional<HistoryMenu>.Null(menu => menu.Exists());
    public static CustomOptional<HistoryMenuButton> HistoryMenuButton = CustomOptional<HistoryMenuButton>.Null();
    private static Dictionary<byte, GameData.PlayerOutfit> _outfits = new();

    static HistoryMenuIntermediate()
    {
        Hooks.GameStateHooks.GameStartHook.Bind(HistoryMenuStartKey, _ => StoreOutfits());
    }

    public static void Initialize()
    {
        VentLogger.Trace("Initializing History Menu");
        HistoryMenuButton = HistoryMenuButton.Exists() ? HistoryMenuButton : Menus.HistoryMenuButton.Create();
        HistoryMenu = HistoryMenu.Exists() ? HistoryMenu : Menus.HistoryMenu.Create();
        VentLogger.Trace("Initialized History Menu");
        if (Game.MatchData.GameHistory != null!) AddHistoryEvents(Game.MatchData.GameHistory.Events);
    }

    public static void StoreOutfits()
    {
        _outfits.Clear();
        PlayerControl.AllPlayerControls.ToArray().ForEach(p => _outfits[p.PlayerId] = p.Data.Outfits[PlayerOutfitType.Default]);
    }

    public static void AddHistoryEvents(List<IHistoryEvent> historyEvents)
    {
        HistoryMenu = HistoryMenu.Exists() ? HistoryMenu : Menus.HistoryMenu.Create();
        if (!HistoryMenu.Exists()) return;
        HistoryMenu menu = HistoryMenu.Get();
        menu.SetHistoryEvents(historyEvents);
    }


    public static Optional<GameData.PlayerOutfit> GetOutfit(byte playerId) => _outfits.GetOptional(playerId);
}