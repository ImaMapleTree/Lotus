using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.Managers.History.Events;
using VentLib.Logging;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace TOHTOR.GUI.Menus;

public class HistoryMenuIntermediate
{
    public static CustomOptional<HistoryMenu> HistoryMenu = CustomOptional<HistoryMenu>.Null(menu => menu.Exists());
    public static CustomOptional<HistoryMenuButton> HistoryMenuButton = CustomOptional<HistoryMenuButton>.Null();
    private static Dictionary<byte, GameData.PlayerOutfit> _outfits = new();

    public static void Initialize()
    {
        VentLogger.Trace("Initializing History Menu");
        HistoryMenuButton = HistoryMenuButton.Exists() ? HistoryMenuButton : Menus.HistoryMenuButton.Create();
        HistoryMenu = HistoryMenu.Exists() ? HistoryMenu : Menus.HistoryMenu.Create();
        VentLogger.Trace("Initialized History Menu");
        if (Game.GameHistory != null!) AddHistoryEvents(Game.GameHistory.Events);
    }

    public static void StoreOutfits()
    {
        _outfits.Clear();
        Game.GetAlivePlayers().ForEach(p => _outfits[p.PlayerId] = p.Data.Outfits[PlayerOutfitType.Default]);
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